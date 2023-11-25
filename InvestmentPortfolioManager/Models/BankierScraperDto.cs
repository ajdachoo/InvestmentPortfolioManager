namespace InvestmentPortfolioManager.Models
{
    public class BankierScraperDto
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Price { get; set; }

        public BankierScraperDto(string name, string ticker, string price)
        {
            Name = name;
            Ticker = ticker;
            Price = price;
        }
    }
}
