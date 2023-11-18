namespace InvestmentPortfolioManager.Entities
{
    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Currency { get; set; }
    }
}
