using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Models;
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

        [HttpGet("{currency}/namelist")]
        public ActionResult<IEnumerable<AssetName>> GetAssetsNamelist([FromRoute] string currency)
        {
            var assets = _assetService.GetAllNames(currency);

            return Ok(assets);
        }

        [HttpGet]
        public ActionResult<PagedResult<AssetDto>> GetAssets([FromQuery]AssetQuery query)
        {
            var assets = _assetService.GetAll(query);

            return Ok(assets);
        }

        [HttpGet("{currency}/{id}")]
        public ActionResult<AssetDto> GetAssetById([FromRoute]string currency, [FromRoute]int id)
        {
            var asset = _assetService.GetById(id, currency);

            return Ok(asset);
        }
    }
}
