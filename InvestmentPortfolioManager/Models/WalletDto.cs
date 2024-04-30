using InvestmentPortfolioManager.Entities;

namespace InvestmentPortfolioManager.Models
{
    public class WalletDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalCost { get; set; }
        public string Currency { get; set; }
        public List<AssetPosition> AssetPositions { get; set; }
        public List<AssetCategoryPosition> AssetCategoryPositions { get; set; }
    }
}