namespace InvestmentPortfolioManager.Models
{
    public class BankierScraperStockDto
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Price { get; set; }

        public BankierScraperStockDto(string name, string ticker, string price)
        {
            Name = name;
            Ticker = ticker;
            Price = price;
        }
    }
}
