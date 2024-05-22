using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/asset/{currency}")]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet("namelist")]
        public IActionResult GetAssetsNamelist([FromRoute] string currency)
        {
            var assets = _assetService.GetAllNames(currency);

            return Ok(assets);
        }

        [HttpGet]
        public IActionResult GetAssets([FromRoute] string currency)
        {
            var assets = _assetService.GetAll(currency).ToList();

            return Ok(assets);
        }

        [HttpGet("{id}")]
        public IActionResult GetAssetById([FromRoute]string currency, [FromRoute]int id)
        {
            var asset = _assetService.GetById(id, currency);

            return Ok(asset);
        }
    }
}
