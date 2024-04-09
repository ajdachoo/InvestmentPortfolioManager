using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace InvestmentPortfolioManager.Services
{
    public interface ISlickchartsScraperService
    {
        public Task UpdateAssets(CancellationToken cancellationToken);
    }

    public class SlickchartsScraperService : ISlickchartsScraperService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly string BaseURL = "https://www.slickcharts.com/sp500";

        public SlickchartsScraperService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var assetCategory = await _dbContext.AssetCategories.FirstOrDefaultAsync(a => a.Name == AssetCategoryEnum.USStocks.ToString(), cancellationToken);

                    if (assetCategory is null)
                    {
                        throw new NotFoundException($"Category \"{AssetCategoryEnum.USStocks}\" does not exist");
                    }

                    var dbAssets = await _dbContext.Assets.Where(a => a.CategoryId == assetCategory.Id).ToListAsync(cancellationToken);
                    var stocks = GetSP500StocksData();
                    var updateDate = DateTime.UtcNow;
                    var cultureInfo = new CultureInfo("en-US");

                    foreach (var stock in stocks)
                    {
                        var dbAsset = dbAssets.FirstOrDefault(a => a.Ticker == stock.Ticker);

                        if (dbAsset is null)
                        {
                            dbAssets.Add(new Asset
                            {
                                CategoryId = assetCategory.Id,
                                Currency = CurrencyEnum.USD,
                                Name = stock.Name,
                                Ticker = stock.Ticker,
                                CurrentPrice = decimal.Parse(stock.Price, cultureInfo),
                                UpdatedDate = updateDate,
                            });
                        }
                        else
                        {
                            _dbContext.Prices.Add(new Price
                            {
                                AssetId = dbAsset.Id,
                                Date = dbAsset.UpdatedDate,
                                Value = dbAsset.CurrentPrice
                            });

                            dbAsset.UpdatedDate = updateDate;
                            dbAsset.CurrentPrice = decimal.Parse(stock.Price, cultureInfo);
                        }
                    }

                    _dbContext.UpdateRange(dbAssets);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Update {AssetCategoryEnum.USStocks} assets error : {ex.Message}");
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        private IEnumerable<SlickchartsScraperDto> GetSP500StocksData()
        {
            var web = new HtmlWeb();
            var document = web.Load(BaseURL);

            var tableRows = document.QuerySelectorAll("table")[0].QuerySelectorAll("tbody tr");

            foreach (var tableRow in tableRows)
            {
                var tds = tableRow.QuerySelectorAll("td");

                var name = tds[1].InnerText;
                var ticker = tds[2].InnerText;
                var price = tds[4].InnerText.Remove(0, 13);

                yield return new SlickchartsScraperDto(name, ticker, price);
            }
        }
    }
}
