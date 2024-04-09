namespace InvestmentPortfolioManager.Entities
{
    public class Price
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; }
    }
}
