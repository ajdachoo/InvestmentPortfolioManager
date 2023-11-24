using InvestmentPortfolioManager.Entities;

namespace InvestmentPortfolioManager.Services
{
    public interface IPolygonAPIService
    {

    }
    public class PolygonAPIService : IPolygonAPIService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public PolygonAPIService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
