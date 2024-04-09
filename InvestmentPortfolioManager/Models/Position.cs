namespace InvestmentPortfolioManager.Models
{
    public class Position
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string Ticker { get; set; }
        public decimal Quantity { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AvgCost { get; set; }
        public decimal Profit { get; set; }
        public double PercentageInWallet { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
