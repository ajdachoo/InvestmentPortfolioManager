namespace InvestmentPortfolioManager.Models
{
    public class BankierScraperForexDto
    {
        public string Ticker { get; set; }
        public string Price { get; set; }

        public BankierScraperForexDto(string ticker, string price)
        {
            Ticker = ticker;
            Price = price;
        }
    }
}
