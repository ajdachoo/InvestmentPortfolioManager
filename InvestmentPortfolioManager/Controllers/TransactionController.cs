using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/wallet/{walletId}/transaction")]
    public class TransactionController : ControllerBase
    {
        public TransactionController()
        {
            
        }

        [HttpPost]
        public ActionResult CreateTransaction()
        {
            return Ok();
        }

        [HttpDelete("{transactionId}")]
        public ActionResult DeleteTransaction([FromRoute] int transactionId)
        {
            return Ok();
        }
    }
}
