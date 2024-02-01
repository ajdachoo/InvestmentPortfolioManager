using InvestmentPortfolioManager.Models;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/wallet/{walletId}/transaction")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public ActionResult CreateTransaction([FromBody] CreateTransactionDto createTransactionDto, [FromRoute] int walletId)
        {
            var transactionId = _transactionService.Create(createTransactionDto, walletId);
            
            return Created($"api/wallet/{walletId}/transaction/{transactionId}", null);
        }

        [HttpDelete("{transactionId}")]
        public ActionResult DeleteTransaction([FromRoute] int transactionId)
        {
            return Ok();
        }
    }
}
