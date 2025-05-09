using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.DTOs;
using TransactionService.Application.Services;

namespace TransactionService.API.Controllers
{
    /// <summary>
    /// API controller for managing financial transactions
    /// </summary>
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
        [HttpPost]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var transaction = await _transactionService.CreateTransactionAsync(request);
            
            return CreatedAtAction(
                nameof(GetTransaction),
                new { request = new GetTransactionRequest { TransactionExternalId = transaction.TransactionExternalId } }, 
                transaction);
        }

        /// <summary>
        /// Retrieves a transaction by its external identifier and optional creation date
        /// </summary>
        /// <param name="request">The transaction retrieval request</param>
        /// <returns>The transaction information</returns>
        /// <response code="200">Returns the requested transaction</response>
        /// <response code="404">If the transaction is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransaction([FromBody] GetTransactionRequest request)
        {
            if (request == null || request.TransactionExternalId == Guid.Empty)
                return BadRequest("TransactionExternalId is required");

            var transaction = await _transactionService.GetTransactionAsync(request);
            
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }
    }
} 