using System.Threading.Tasks;
using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AntiFraudService.API.Controllers
{
    /// <summary>
    /// Controller for handling transaction validation operations
    /// This would typically be invoked by Kafka consumers in a production environment,
    /// but we expose REST endpoints for testing purposes
    /// </summary>
    [ApiController]
    [Route("api/antifraud")]
    public class AntiFraudController : ControllerBase
    {
        private readonly IAntiFraudApplicationService _antiFraudService;
        private readonly ILogger<AntiFraudController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AntiFraudController"/> class
        /// </summary>
        /// <param name="antiFraudService">The anti-fraud application service</param>
        /// <param name="logger">The logger</param>
        public AntiFraudController(
            IAntiFraudApplicationService antiFraudService,
            ILogger<AntiFraudController> logger)
        {
            _antiFraudService = antiFraudService;
            _logger = logger;
        }

        /// <summary>
        /// Processes a transaction validation request
        /// </summary>
        /// <param name="request">The transaction validation request</param>
        /// <returns>No content if successful</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateTransaction([FromBody] TransactionValidationRequest request)
        {
            _logger.LogInformation(
                "Received validation request for transaction {TransactionId}", 
                request.TransactionExternalId);

            await _antiFraudService.ProcessTransactionValidationRequestAsync(request);
            
            return NoContent();
        }
    }
} 