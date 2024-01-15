using InvestmentPortfolioManager.Models;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentPortfolioManager.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public ActionResult RegisterUser([FromBody] RegisterUserDto registerUserDto)
        {
            _accountService.RegisterUser(registerUserDto);

            return Ok();
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginDto loginDto)
        {
            string token = _accountService.GenerateJwt(loginDto);
            
            return Ok(token);
        }

        [HttpDelete("{userId}")]
        public ActionResult DeleteUser()
        {
            return Ok();
        }

        [HttpGet("{userId}")]
        public ActionResult GetUser()
        {
            return Ok();
        }

        [HttpPut("{userId}")]
        public ActionResult UpdateUser()
        {
            return Ok();
        }
    }
}
