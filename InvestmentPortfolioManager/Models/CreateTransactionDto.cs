using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Models
{
    public class CreateTransactionDto
    {
        public int AssetId { get; set; }
        public string Type { get; set; }
        public double Quantity { get; set; }
        public decimal InitialValue { get; set; }
        public string TransactionDate { get; set; }
    }
}
