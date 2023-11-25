using InvestmentPortfolioManager.Models;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ICoinGeckoAPIService _cryptocurrencyAPIService;
        private readonly IBankierScraperService _bankierScraperService;

        public AdminController(ICoinGeckoAPIService cryptocurrencyAPIService, IBankierScraperService bankierScraperService)
        {
            _cryptocurrencyAPIService = cryptocurrencyAPIService;
            _bankierScraperService = bankierScraperService;
        }

        [HttpGet]
        public ActionResult Update()
        {
            return Ok();
        }
    }
}
