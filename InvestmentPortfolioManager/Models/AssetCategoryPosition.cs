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
        public decimal Proceeds { get; set; }
        public double PercentageChange24h { get; set; }
        public double PercentageChange7d { get; set; }
        public double PercentageChange1m { get; set; }
        public double PercentageChange1y { get; set; }
    }
}
