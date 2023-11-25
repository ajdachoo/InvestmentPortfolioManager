using InvestmentPortfolioManager.Entities;

namespace InvestmentPortfolioManager.Services
{
    public interface ITwelveDataAPIService
    {

    }
    public class TwelveDataAPIService : ITwelveDataAPIService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public TwelveDataAPIService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateAssets(CancellationToken cancellationToken)
        {

        }
    }
}
