using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ICoinGeckoAPIService _cryptocurrencyAPIService;

        public AdminController(ICoinGeckoAPIService cryptocurrencyAPIService)
        {
            _cryptocurrencyAPIService = cryptocurrencyAPIService;
        }

        [HttpGet]
        public ActionResult Update()
        {
            return Ok();
        }
    }
}
