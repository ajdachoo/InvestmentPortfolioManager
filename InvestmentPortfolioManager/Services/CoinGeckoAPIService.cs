using Flurl;
using Flurl.Http;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace InvestmentPortfolioManager.Services
{
    public interface ICoinGeckoAPIService
    {
        public Task UpdateAssets(CancellationToken cancellationToken);
    }
    public class CoinGeckoAPIService : ICoinGeckoAPIService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public CoinGeckoAPIService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateAssets(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try 
                {
                    var APIResponse = await "https://api.coingecko.com/api/v3/coins/markets"
                    .WithHeader("user-agent", "something")
                    .SetQueryParams(new
                    {
                        vs_currency = "usd",
                        order = "market_cap_desc",
                        per_page = 200,
                        page = 1,
                        sparkline = false,
                        locale = "pl",
                        precision = 8
                    })
                    .GetAsync(cancellationToken)
                    .ReceiveString();

                    var updateDate = DateTime.UtcNow;

                    var cryptocurrencyAssetsDtos = JsonConvert.DeserializeObject<List<CoinGeckoAPIDto>>(APIResponse);

                    var assetCategory = await _dbContext.AssetCategories.FirstOrDefaultAsync(a => a.Name == AssetCategoryEnum.Cryptocurrencies.ToString(), cancellationToken);

                    if (assetCategory is null)
                    {
                        throw new NotFoundException($"Category \"{AssetCategoryEnum.Cryptocurrencies}\" does not exist");
                    }

                    var dbAssets = await _dbContext.Assets.Where(a => a.CategoryId == assetCategory.Id).ToListAsync(cancellationToken);

                    foreach (var item in cryptocurrencyAssetsDtos)
                    {
                        var dbAsset = dbAssets.FirstOrDefault(a => a.Ticker == item.Symbol.ToUpper());

                        if (dbAsset is null)
                        {
                            dbAssets.Add(new Asset
                            {
                                Ticker = item.Symbol.ToUpper(),
                                UpdatedDate = updateDate,
                                CurrentPrice = item.Current_price,
                                Currency = CurrencyEnum.USD,
                                CategoryId = assetCategory.Id,
                                Name = item.Name,
                            });
                        }
                        else
                        {
                            if(dbAsset.CurrentPrice != item.Current_price)
                            {
                                _dbContext.Prices.Add(new Price
                                {
                                    AssetId = dbAsset.Id,
                                    Date = dbAsset.UpdatedDate,
                                    Value = dbAsset.CurrentPrice
                                });
                            }

                            dbAsset.UpdatedDate = updateDate;
                            dbAsset.CurrentPrice = item.Current_price;
                        }
                    }
                    _dbContext.UpdateRange(dbAssets);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch(Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Update {AssetCategoryEnum.Cryptocurrencies} assets error : {ex.Message}");
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                await Task.Delay(60000, cancellationToken);
            }
        }
    }
}
