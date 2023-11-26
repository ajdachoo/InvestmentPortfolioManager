﻿using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
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
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private const string WIGStocksBaseURL = "https://www.bankier.pl/inwestowanie/profile/quote.html?symbol=WIG";
        private const string ForexBaseURL = "https://www.bankier.pl/waluty/kursy-walut/forex";

        public BankierScraperService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateStockAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var dbAssets = _dbContext.Assets.Where(a => a.Category == AssetCategoryEnum.PolishStocks).ToList();
                    var stocks = GetWIGStocksData();
                    var updateDate = DateTime.UtcNow;

                    foreach (var stock in stocks)
                    {
                        var dbAsset = dbAssets.FirstOrDefault(a => a.Ticker == stock.Ticker);

                        if (dbAsset is null)
                        {
                            dbAssets.Add(new Asset
                            {
                                Category = AssetCategoryEnum.PolishStocks,
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

        public async Task UpdateForexAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var dbAssets = _dbContext.Assets.Where(a => a.Category == AssetCategoryEnum.PhysicalCurrencies).ToList();
                    var forexAssets = GetForexData();
                    var updateDate = DateTime.UtcNow;

                    foreach (var forexAsset in forexAssets)
                    {
                        var assets = GetTwoForexPairs(forexAsset, updateDate);

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

                    _dbContext.UpdateRange(dbAssets);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Update physical currency assets error : {ex.Message}");
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                await Task.Delay(60000, cancellationToken);
            }
        }

        private IEnumerable<Asset> GetTwoForexPairs(BankierScraperForexDto asset, DateTime updateDate)
        {
            var splitTicker = asset.Ticker.Split("/");
            var price = decimal.Parse(asset.Price);

            var asset1 = new Asset()
            {
                Name = asset.Ticker,
                Category = AssetCategoryEnum.PhysicalCurrencies,
                Currency = Enum.Parse<CurrencyEnum>(splitTicker[1]),
                Price = price,
                Ticker = asset.Ticker,
                UpdatedDate = updateDate,
            };

            var asset2 = new Asset()
            {
                Name = $"{splitTicker[1]}/{splitTicker[0]}",
                Category = AssetCategoryEnum.PhysicalCurrencies,
                Currency = Enum.Parse<CurrencyEnum>(splitTicker[0]),
                Price = Math.Round(1 / price, 4),
                Ticker = $"{splitTicker[1]}/{splitTicker[0]}",
                UpdatedDate = updateDate,
            };

            return new List<Asset>() { asset1, asset2 };
        }

        private static IEnumerable<BankierScraperStockDto> GetWIGStocksData()
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
