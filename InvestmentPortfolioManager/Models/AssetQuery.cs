namespace InvestmentPortfolioManager.Models
{
    public class AssetQuery
    {
        public string SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string Currency { get; set; }
    }
}
