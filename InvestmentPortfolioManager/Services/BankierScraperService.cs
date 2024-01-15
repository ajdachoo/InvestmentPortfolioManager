using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Services
{
    public interface IBankierScraperService
    {
        public Task UpdateStockAssets(CancellationToken cancellationToken);
        public Task UpdateForexAssets(CancellationToken cancellationToken);
    }
    public class BankierScraperService : IBankierScraperService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext1;
        private const string WIGStocksBaseURL = "https://www.bankier.pl/inwestowanie/profile/quote.html?symbol=WIG";
        private readonly InvestmentPortfolioManagerDbContext _dbContext2;
        private const string ForexBaseURL = "https://www.bankier.pl/waluty/kursy-walut/forex";

        public BankierScraperService(InvestmentPortfolioManagerDbContext dbContext1, InvestmentPortfolioManagerDbContext dbContext2)
        {
            _dbContext1 = dbContext1;
            _dbContext2 = dbContext2;
        }

        public async Task UpdateStockAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var assetCategory = await _dbContext1.AssetCategories.FirstOrDefaultAsync(a => a.Name == AssetCategoryEnum.PolishStocks.ToString(), cancellationToken);

                    if(assetCategory is null)
                    {
                        throw new NotFoundException($"Category \"{AssetCategoryEnum.PolishStocks}\" does not exist");
                    }

                    var dbAssets = await _dbContext1.Assets.Where(a => a.CategoryId == assetCategory.Id).ToListAsync(cancellationToken);
                    var stocks = GetWIGStocksData();
                    var updateDate = DateTime.UtcNow;

                    foreach (var stock in stocks)
                    {
                        var dbAsset = dbAssets.FirstOrDefault(a => a.Ticker == stock.Ticker);

                        if (dbAsset is null)
                        {
                            dbAssets.Add(new Asset
                            {
                                CategoryId = assetCategory.Id,
                                Currency = CurrencyEnum.PLN,
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

                    _dbContext1.UpdateRange(dbAssets);
                    await _dbContext1.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Update {AssetCategoryEnum.PolishStocks} assets error : {ex.Message}");
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        public async Task UpdateForexAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var assetCategory = await _dbContext2.AssetCategories.FirstOrDefaultAsync(a => a.Name == AssetCategoryEnum.PhysicalCurrencies.ToString(), cancellationToken);

                    if (assetCategory is null)
                    {
                        throw new NotFoundException($"Category \"{AssetCategoryEnum.PhysicalCurrencies}\" does not exist");
                    }

                    var dbAssets = await _dbContext2.Assets.Where(a => a.CategoryId == assetCategory.Id).ToListAsync(cancellationToken);
                    var forexAssets = GetForexData();
                    var updateDate = DateTime.UtcNow;

                    foreach (var forexAsset in forexAssets)
                    {
                        var assets = GetTwoForexPairs(forexAsset, updateDate, assetCategory.Id);

                        foreach (var asset in assets)
                        {
                            var dbAsset = dbAssets.FirstOrDefault(a => a.Ticker == asset.Ticker);
                            if(dbAsset is null)
                            {
                                dbAssets.Add(asset);
                            }
                            else
                            {
                                dbAsset.Price = asset.Price;
                                dbAsset.UpdatedDate = asset.UpdatedDate;
                            }
                        }

                    }

                    _dbContext2.UpdateRange(dbAssets);
                    await _dbContext2.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Update {AssetCategoryEnum.PhysicalCurrencies} assets error : {ex.Message}");
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        private IEnumerable<Asset> GetTwoForexPairs(BankierScraperForexDto asset, DateTime updateDate, int assetCategoryId)
        {
            var splitTicker = asset.Ticker.Split("/");
            var price = decimal.Parse(asset.Price);

            var asset1 = new Asset()
            {
                Name = asset.Ticker,
                CategoryId = assetCategoryId,
                Currency = Enum.Parse<CurrencyEnum>(splitTicker[1]),
                Price = price,
                Ticker = asset.Ticker,
                UpdatedDate = updateDate,
            };

            yield return asset1;

            var asset2 = new Asset()
            {
                Name = $"{splitTicker[1]}/{splitTicker[0]}",
                CategoryId = assetCategoryId,
                Currency = Enum.Parse<CurrencyEnum>(splitTicker[0]),
                Price = Math.Round(1 / price, 4),
                Ticker = $"{splitTicker[1]}/{splitTicker[0]}",
                UpdatedDate = updateDate,
            };

            yield return asset2;
        }

        private IEnumerable<BankierScraperStockDto> GetWIGStocksData()
        {
            var web = new HtmlWeb();
            var document = web.Load(WIGStocksBaseURL);

            var tableRows = document.QuerySelectorAll("table").ElementAt(1).QuerySelectorAll("tr").Skip(1);

            foreach(var tableRow in tableRows)
            {
                var tds = tableRow.QuerySelectorAll("td");

                var name = tds[0].QuerySelector("a").Attributes["title"].Value;
                var ticker = tds[1].InnerHtml;
                var price = tds[2].InnerHtml;

                yield return new BankierScraperStockDto(name, ticker, price);
            }

        }

        private IEnumerable<BankierScraperForexDto> GetForexData()
        {
            var web = new HtmlWeb();
            var document = web.Load(ForexBaseURL);

            var tableRows = document.QuerySelectorAll("table").ElementAt(0).QuerySelectorAll("tbody tr");
            var indexes = new List<int>() { 0, 1, 3, 2, 4, 21, 19, 45, 38, 33 };

            foreach(var index in indexes)
            {
                var tableRow = tableRows.ElementAt(index);
                var tds = tableRow.QuerySelectorAll("td");

                var ticker = tds[0].QuerySelector("a").InnerHtml;
                var price = tds[1].InnerHtml;

                yield return new BankierScraperForexDto(ticker, price);
            }

        } 
    }
}
