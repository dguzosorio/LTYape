using System;
using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Services;
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
        /// <returns>No content (204) if the validation was processed successfully.</returns>
        /// <response code="204">The validation request was processed successfully.</response>
        /// <response code="400">The validation request was invalid.</response>
        /// <response code="500">An unexpected error occurred while processing the validation.</response>
        [HttpPost("validate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateTransaction([FromBody] TransactionValidationRequest request)
        {
            try
            {
                if (request == null || request.TransactionExternalId == Guid.Empty)
                {
                    _logger.LogWarning("Received invalid validation request");
                    return BadRequest("The validation request is invalid or has an incorrect format");
                }
                
                _logger.LogInformation(
                    "Received validation request for transaction {TransactionId}", 
                    request.TransactionExternalId);

                await _antiFraudService.ProcessTransactionValidationRequestAsync(request);
                
                return NoContent();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                // Handle duplicate key errors specifically
                _logger.LogWarning(
                    "Attempted to validate a transaction that already exists: {TransactionId}. This is normal if the transaction was processed previously.",
                    request.TransactionExternalId);
                
                // Return 204 instead of error since this is not considered a real error
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Unexpected error processing transaction validation {TransactionId}: {ErrorMessage}",
                    request.TransactionExternalId, ex.Message);
                
                // Don't expose internal error details to the client, just return a generic message
                return StatusCode(500, "An error occurred while processing the validation request");
            }
        }
    }
} 