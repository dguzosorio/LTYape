using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.DTOs;
using TransactionService.Application.Exceptions;
using TransactionService.Application.Services;
using TransactionService.Domain.Exceptions;
using FluentValidation;

namespace TransactionService.API.Controllers
{
    /// <summary>
    /// API controller for managing financial transactions
    /// </summary>
    /// <remarks>
    /// All operations use POST method to allow complex request bodies:
    /// - POST /api/Transactions/set - Creates a new transaction
    /// - POST /api/Transactions/get - Retrieves transaction details
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionApplicationService _transactionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionsController"/> class
        /// </summary>
        /// <param name="transactionService">The transaction application service</param>
        public TransactionsController(ITransactionApplicationService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Creates a new financial transaction
        /// </summary>
        /// <param name="request">The transaction creation request</param>
        /// <returns>The created transaction information</returns>
        /// <response code="201">Returns the newly created transaction</response>
        /// <response code="400">If the request is invalid</response>
        /// <remarks>POST: /api/Transactions/set</remarks>
        [HttpPost("set")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(request);
                
                return CreatedAtAction(
                    "GetTransaction",
                    "Transactions",
                    new { get = "" },
                    transaction);
            }
            catch (ValidationException ex)
            {
                // Specific handling for FluentValidation validation errors
                return BadRequest(new 
                { 
                    errors = ex.Errors.Select(e => new 
                    { 
                        property = e.PropertyName, 
                        message = e.ErrorMessage 
                    })
                });
            }
            catch (TransactionDomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidRequestException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a transaction by its external identifier and optional creation date
        /// </summary>
        /// <param name="request">The transaction retrieval request</param>
        /// <returns>The transaction information</returns>
        /// <response code="200">Returns the requested transaction</response>
        /// <response code="404">If the transaction is not found</response>
        /// <remarks>POST: /api/Transactions/get</remarks>
        [HttpPost("get")]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransaction([FromBody] GetTransactionRequest request)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionAsync(request);
                
                if (transaction == null)
                    return NotFound();

                return Ok(transaction);
            }
            catch (ValidationException ex)
            {
                // Specific handling for FluentValidation validation errors
                return BadRequest(new 
                { 
                    errors = ex.Errors.Select(e => new 
                    { 
                        property = e.PropertyName, 
                        message = e.ErrorMessage 
                    })
                });
            }
        }
    }
} 