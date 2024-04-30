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

        [HttpGet("{walletId}")]
        public ActionResult<WalletDto> GetWalletById([FromRoute] int walletId)
        {
            var wallet = _walletService.GetWallet(walletId);

            return wallet;
        }

        [HttpGet]
        public ActionResult<List<WalletDto>> GetAllWallets([FromRoute] int? userId)
        {
            var walletDtos = _walletService.GetAll(userId).ToList();

            return walletDtos;
        }
    }
}
