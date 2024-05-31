namespace InvestmentPortfolioManager.Models
{
    public class AssetPosition
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public int AssetCategoryId { get; set; }
        public string AssetCategoryName { get; set; }
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Proceeds { get; set; }
        public decimal AvgCost { get; set; }
        public decimal Profit { get; set; }
        public double PercentageInWallet { get; set; }
        public DateTime UpdatedDate { get; set; }
        public double PercentageChange24h { get; set; }
        public double PercentageChange7d { get; set; }
        public double PercentageChange1m { get; set; }
        public double PercentageChange1y { get; set; }
    }
}
