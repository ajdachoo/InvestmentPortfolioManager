using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        public WalletController()
        {
            
        }

        [HttpPost]
        public ActionResult CreateWallet()
        {
            return Ok();
        }

        [HttpDelete("{walletId}")]
        public ActionResult DeleteWallet([FromRoute] int walletId)
        {
            return Ok();
        }

        [HttpPost("{walletId}")]
        public ActionResult AddTransaction()
        {
            return Ok();
        }
    }
}
