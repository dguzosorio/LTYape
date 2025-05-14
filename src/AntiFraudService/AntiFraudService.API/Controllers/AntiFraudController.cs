using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AntiFraudService.API.Controllers
{
    /// <summary>
    /// Controller for handling transaction validation operations.
    /// This would typically be invoked by Kafka consumers in a production environment,
    /// but we expose REST endpoints for testing and debugging purposes.
    /// </summary>
    [ApiController]
    [Route("api/antifraud")]
    public class AntiFraudController : ControllerBase
    {
        private readonly IAntiFraudApplicationService _antiFraudService;
        private readonly ILogger<AntiFraudController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudController"/> class.
        /// </summary>
        /// <param name="antiFraudService">The anti-fraud application service.</param>
        /// <param name="logger">The logger instance.</param>
        public AntiFraudController(
            IAntiFraudApplicationService antiFraudService,
            ILogger<AntiFraudController> logger)
        {
            _antiFraudService = antiFraudService;
            _logger = logger;
        }

        /// <summary>
        /// Processes a transaction validation request and performs fraud checks.
        /// </summary>
        /// <param name="request">The transaction validation request containing transaction details.</param>
        /// <returns>
        /// - 204 No Content if the validation was processed successfully.
        /// - 409 Conflict if the transaction was already validated.
        /// - 400 Bad Request if the validation request is invalid.
        /// - 500 Internal Server Error if an unexpected error occurred.
        /// </returns>
        /// <response code="204">The validation request was processed successfully.</response>
        /// <response code="409">The transaction has already been validated.</response>
        /// <response code="400">The validation request was invalid.</response>
        /// <response code="500">An unexpected error occurred while processing the validation.</response>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateTransaction([FromBody] TransactionValidationRequest request)
        {
            // La validación del modelo ahora es manejada automáticamente por FluentValidation
            // Si el modelo no es válido, ASP.NET Core devuelve automáticamente una respuesta 400 Bad Request
            
            try
            {
                _logger.LogInformation(
                    "Received validation request for transaction {TransactionId}, Amount: {Amount}, From: {SourceAccount} To: {TargetAccount}", 
                    request.TransactionExternalId, 
                    request.Value,
                    request.SourceAccountId,
                    request.TargetAccountId);

                // Try to get existing validation before processing
                try
                {
                    var existingValidation = await _antiFraudService.GetTransactionValidationAsync(request.TransactionExternalId);
                    
                    if (existingValidation != null)
                    {
                        _logger.LogInformation(
                            "Transaction {TransactionId} was already validated with result: {Result}", 
                            request.TransactionExternalId, 
                            existingValidation.Result);
                            
                        return Conflict(new { 
                            message = "Transaction has already been validated", 
                            transactionId = request.TransactionExternalId.ToString(),
                            result = existingValidation.Result.ToString(),
                            validationDate = existingValidation.ValidationDate
                        });
                    }
                }
                catch (KeyNotFoundException)
                {
                    // Transaction has not been validated yet, continue with the process
                }

                await _antiFraudService.ProcessTransactionValidationRequestAsync(request);
                
                return NoContent();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // This may happen if concurrent validations are attempted
                _logger.LogInformation(
                    "Concurrent validation detected for transaction {TransactionId}", 
                    request.TransactionExternalId);
                
                return Conflict(new { 
                    message = "Transaction has already been validated by a concurrent request", 
                    transactionId = request.TransactionExternalId.ToString()
                });
            }
            catch (Exception ex)
            {
                // Log the full exception
                _logger.LogError(
                    ex, 
                    "Unexpected error processing transaction validation {TransactionId}: {ErrorMessage}",
                    request.TransactionExternalId, ex.Message);
                
                return StatusCode(500, "An error occurred while processing the validation request");
            }
        }
    }
} 