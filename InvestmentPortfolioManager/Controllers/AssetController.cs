using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/asset")]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [Route("{currency}")]
        public IActionResult GetAssets([FromRoute] string currency)
        {
            var assets = _assetService.GetAll(currency);

            return Ok(assets);
        }

        [Route("{currency}/{category}")]
        public IActionResult GetAssetsByCategory([FromRoute]string currency, [FromRoute]string category)
        {
            var assets = _assetService.GetAssetsByCategory(category, currency);

            return Ok(assets);
        }
    }
}
