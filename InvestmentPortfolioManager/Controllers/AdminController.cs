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
        private readonly ISlickchartsScraperService _slickchartsScraperService;

        public AdminController(ICoinGeckoAPIService cryptocurrencyAPIService, IBankierScraperService bankierScraperService, ISlickchartsScraperService slickchartsScraperService)
        {
            _cryptocurrencyAPIService = cryptocurrencyAPIService;
            _bankierScraperService = bankierScraperService;
            _slickchartsScraperService = slickchartsScraperService;
        }

        //[HttpGet]
        //public ActionResult Update()
        //{
        //    return Ok();
        //}
    }
}
