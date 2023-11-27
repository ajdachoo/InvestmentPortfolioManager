namespace InvestmentPortfolioManager.Models
{
    public class SlickchartsScraperDto
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Price { get; set; }

        public SlickchartsScraperDto(string name, string ticker, string price)
        {
            Name = name;
            Ticker = ticker;
            Price = price;
        }
    }
}
