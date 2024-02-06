using InvestmentPortfolioManager.Models;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/wallet")]
    [Route("api/{userId?}/wallet")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost]
        public ActionResult CreateWallet([FromBody] CreateWalletDto createWalletDto, [FromRoute] int? userId)
        {
            var walletId = _walletService.Create(createWalletDto, userId);

            return Created($"api/wallet/{walletId}", null);
        }

        [HttpDelete("{walletId}")]
        public ActionResult DeleteWallet([FromRoute] int walletId)
        {
            _walletService.Delete(walletId);
            
            return NoContent();
        }
    }
}
