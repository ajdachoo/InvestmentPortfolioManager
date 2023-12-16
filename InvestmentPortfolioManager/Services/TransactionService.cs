using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Models;

namespace InvestmentPortfolioManager.Services
{
    public interface ITransactionService
    {
        public int Create(CreateTransactionDto dto);
    }

    public class TransactionService : ITransactionService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public TransactionService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int Create(CreateTransactionDto dto)
        {
            return 0;
        }
    }
}
