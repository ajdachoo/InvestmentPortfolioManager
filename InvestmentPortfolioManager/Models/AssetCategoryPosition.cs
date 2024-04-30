namespace InvestmentPortfolioManager.Models
{
    public class AssetCategoryPosition
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double PercentageInWallet { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalCost { get; set; }
    }
}
