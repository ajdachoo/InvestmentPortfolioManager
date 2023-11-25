using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Services
{
    public interface IBankierScraperService
    {
        public Task UpdateAssets(CancellationToken cancellationToken);
    }
    public class BankierScraperService : IBankierScraperService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private const string BaseURL = "https://www.bankier.pl/inwestowanie/profile/quote.html?symbol=WIG";

        public BankierScraperService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var dbAssets = _dbContext.Assets.Where(a => a.Category == "stocks(POLAND)").ToList();
                    var stocks = GetWIGStocks();
                    var updateDate = DateTime.UtcNow;

                    foreach (var stock in stocks)
                    {
                        var dbAsset = dbAssets.FirstOrDefault(a => a.Ticker == stock.Ticker);

                        if (dbAsset is null)
                        {
                            dbAssets.Add(new Asset
                            {
                                Category = "stocks(POLAND)",
                                Currency = "PLN",
                                Name = stock.Name,
                                Ticker = stock.Ticker,
                                Price = decimal.Parse(stock.Price),
                                UpdatedDate = updateDate,
                            });
                        }
                        else
                        {
                            dbAsset.UpdatedDate = updateDate;
                            dbAsset.Price = decimal.Parse(stock.Price);
                        }
                    }

                    _dbContext.UpdateRange(dbAssets);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Update stocks(POLAND) assets error : {ex.Message}");
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        private static IEnumerable<BankierScraperDto> GetWIGStocks()
        {
            var web = new HtmlWeb();
            var document = web.Load(BaseURL);

            var tableRows = document.QuerySelectorAll("table").ElementAt(1).QuerySelectorAll("tr").Skip(1);

            foreach(var tableRow in tableRows)
            {
                var tds = tableRow.QuerySelectorAll("td");

                var name = tds[0].QuerySelector("a").Attributes["title"].Value;
                var ticker = tds[1].InnerHtml;
                var price = tds[2].InnerHtml;

                yield return new BankierScraperDto(name, ticker, price);
            }

        }
    }
}
