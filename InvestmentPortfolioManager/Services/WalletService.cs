using InvestmentPortfolioManager.Entities;

namespace InvestmentPortfolioManager.Services
{
    public interface IWalletService
    {

    }


    public class WalletService
    {
        private readonly InvestmentPortfolioManagerDbContext _cbContext;

        public WalletService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _cbContext = dbContext;
        }

        public int Create()
        {
            return 0;
        }
    }
}
