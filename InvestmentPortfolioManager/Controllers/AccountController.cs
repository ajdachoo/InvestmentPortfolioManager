using InvestmentPortfolioManager.Models;
using InvestmentPortfolioManager.Services;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("authorize")]
        [Authorize]
        public ActionResult Authorize()
        {
            return Ok();
        }

        [HttpDelete("{userId?}")]
        [Authorize]
        public ActionResult DeleteUser([FromRoute] int? userId)
        {
            _accountService.DeleteUser(userId);

            return NoContent();
        }

        [HttpGet("{userId?}")]
        [Authorize]
        public ActionResult GetUser([FromRoute] int? userId)
        {
            var user = _accountService.GetUserById(userId);
            
            return Ok(user);
        }

        [HttpPut("{userId}")]
        [Authorize]
        public ActionResult UpdateUser()
        {
            return Ok();
        }
    }
}
