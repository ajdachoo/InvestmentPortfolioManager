using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Models
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetTicker { get; set; }
        public int WalletId { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal InitialValue { get; set; }
        public string Currency { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
