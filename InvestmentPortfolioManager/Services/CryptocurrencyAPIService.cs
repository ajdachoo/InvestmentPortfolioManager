using Flurl;
using Flurl.Http;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Models;
using Newtonsoft.Json;

namespace InvestmentPortfolioManager.Services
{
    public interface ICryptocurrencyAPIService
    {
        public Task UpdateAssets();
    }
    public class CryptocurrencyAPIService : ICryptocurrencyAPIService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public CryptocurrencyAPIService(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UpdateAssets()
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
                .GetAsync()
                .ReceiveString();

            var updateDate = DateTime.UtcNow;

            var cryptocurrencyAssetsDtos = JsonConvert.DeserializeObject<List<CryptocurrencyAssetDto>>(APIResponse);

            var dbAssets = _dbContext.Assets.Where(a => a.Category == "cryptocurrencies").ToList();
            
            foreach(var item in cryptocurrencyAssetsDtos)
            {
                var dbCryptocurrenciesAsset = dbAssets.FirstOrDefault(a => a.Ticker == item.Symbol.ToUpper());

                if(dbCryptocurrenciesAsset is null)
                {
                    dbAssets.Add(new Asset
                    {
                        Ticker = item.Symbol.ToUpper(),
                        UpdatedDate = updateDate,
                        Price = item.Current_price,
                        Currency = "USD",
                        Category = "cryptocurrencies",
                        Name = item.Name,
                    });
                }
                else
                {
                    dbCryptocurrenciesAsset.UpdatedDate = updateDate;
                    dbCryptocurrenciesAsset.Price = item.Current_price;
                }
            }
            _dbContext.UpdateRange(dbAssets);
            _dbContext.SaveChanges();
        }
    }
}
