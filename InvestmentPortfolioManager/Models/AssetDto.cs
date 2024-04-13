using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Models
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Category { get; set; }
        public decimal CurrentPrice { get; set; }
        public double PercentageChange24h { get; set; }
        public double PercentageChange7d { get; set; }
        public double PercentageChange1m { get; set; }
        public double PercentageChange1y { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Currency { get; set; }
    }
}
