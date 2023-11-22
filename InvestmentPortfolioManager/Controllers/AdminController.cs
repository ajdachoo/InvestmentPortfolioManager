using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ICryptocurrencyAPIService _cryptocurrencyAPIService;

        public AdminController(ICryptocurrencyAPIService cryptocurrencyAPIService)
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
