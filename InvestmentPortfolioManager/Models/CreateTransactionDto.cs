using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Models
{
    public class CreateTransactionDto
    {
        public int AssetId { get; set; }
        public int WalletId { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public double Quantity { get; set; }
        public decimal InitialValue { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
