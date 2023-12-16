using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Models;

namespace InvestmentPortfolioManager.Services
{
   public interface IAccountService
    {

    }

    public class AccountService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public AccountService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int RegisterUser(RegisterUserDto registerUserDto)
        {
            return 0;
        }
    }
}
