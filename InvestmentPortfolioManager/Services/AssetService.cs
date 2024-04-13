using AutoMapper;
using AutoMapper.Configuration.Annotations;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Services
{
    public interface IAssetService
    {
        public IEnumerable<AssetDto> GetAll(string currency = "USD");
        public IEnumerable<AssetDto> GetAssetsByCategory(string asssetCategory, string currency = "USD");
        public AssetDto GetById(int id, string currency = "USD");
    }

    public class AssetService : IAssetService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly IMapper _mapper;

        public AssetService(InvestmentPortfolioManagerDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public IEnumerable<AssetDto> GetAll(string currency = "USD")
        {
            var assets = _dbContext.Assets.Include(a => a.Category).ToList();

            var assetDtos = GetAssetDtos(assets, currency);

            return assetDtos;
        }

        public AssetDto GetById(int id, string currency = "USD")
        {
            var asset = _dbContext.Assets.Include(a => a.Category).FirstOrDefault(a => a.Id == id);

            if (asset is null)
            {
                throw new NotFoundException("Asset not found.");
            }

            var assetDto = GetAssetDtos(new[] { asset }, currency);

            return assetDto.First();
        }

        public IEnumerable<AssetDto> GetAssetsByCategory(string asssetCategory, string currency = "USD")
        {
            var assetCategory = _dbContext.AssetCategories.FirstOrDefault(c => c.Name.ToLower() == asssetCategory.ToLower());

            if(assetCategory is null)
            {
                throw new BadRequestException("Asset category not found.");
            }

            var assets = _dbContext.Assets.Where(a => a.CategoryId == assetCategory.Id).Include(a => a.Category).ToList();

            var assetDtos = GetAssetDtos(assets, currency);

            return assetDtos;
        }

        private IEnumerable<AssetDto> GetAssetDtos(IEnumerable<Asset> assets, string currency)
        {
            currency = currency.ToUpper();

            if(!Enum.TryParse<CurrencyEnum>(currency, true, out CurrencyEnum currencyEnum))
            {
                throw new BadRequestException("Currency not found.");
            }

            var physicalCurrencyCategoryId = _dbContext.AssetCategories.FirstOrDefault(c => c.Name == AssetCategoryEnum.PhysicalCurrencies.ToString()).Id;

            var currencies = _dbContext.Assets.Where(a => a.CategoryId == physicalCurrencyCategoryId && a.Currency == currencyEnum).ToList();

            var currentDate = DateTime.UtcNow;

            foreach (var asset in assets)
            {
                if(asset.CategoryId == physicalCurrencyCategoryId && asset.Currency != currencyEnum)
                {
                    continue;
                }

                double percentageChange24h = 0, percentageChange7d = 0, percentageChange1m = 0, percentageChange1y = 0;

                if (asset.Currency != currencyEnum)
                {
                    var newCurrency = currencies.FirstOrDefault(a => a.Ticker == $"{asset.Currency}/{currency}");

                    asset.CurrentPrice *= newCurrency.CurrentPrice;
                    asset.Currency = currencyEnum;

                    percentageChange24h = (double)((((GetPriceByClosestDate(currentDate.AddHours(-24), asset.Id).Value * GetPriceByClosestDate(currentDate.AddHours(-24), newCurrency.Id).Value) - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                    percentageChange7d = (double)((((GetPriceByClosestDate(currentDate.AddDays(-7), asset.Id).Value * GetPriceByClosestDate(currentDate.AddDays(-7), newCurrency.Id).Value) - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                    percentageChange1m = (double)((((GetPriceByClosestDate(currentDate.AddMonths(-1), asset.Id).Value * GetPriceByClosestDate(currentDate.AddMonths(-1), newCurrency.Id).Value) - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                    percentageChange1y = (double)((((GetPriceByClosestDate(currentDate.AddYears(-1), asset.Id).Value * GetPriceByClosestDate(currentDate.AddYears(-1), newCurrency.Id).Value) - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                }
                else
                {
                    percentageChange24h = (double)(((GetPriceByClosestDate(currentDate.AddHours(-24), asset.Id).Value - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                    percentageChange7d = (double)(((GetPriceByClosestDate(currentDate.AddDays(-7), asset.Id).Value - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                    percentageChange1m = (double)(((GetPriceByClosestDate(currentDate.AddMonths(-1), asset.Id).Value - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                    percentageChange1y = (double)(((GetPriceByClosestDate(currentDate.AddYears(-1), asset.Id).Value - asset.CurrentPrice) / asset.CurrentPrice) * 100);
                }

                var assetDto = _mapper.Map<AssetDto>(asset);

                assetDto.PercentageChange24h = percentageChange24h;
                assetDto.PercentageChange7d = percentageChange7d;
                assetDto.PercentageChange1m = percentageChange1m;
                assetDto.PercentageChange1y = percentageChange1y;

                yield return assetDto;
            }
        }

        private Price GetPriceByClosestDate(DateTime date, int assetId)
        {
            var prices = _dbContext.Prices.Where(p => p.AssetId == assetId).OrderBy(p => p.Date).ToList();

            if(prices.Count == 0)
            {
                var currentPrice = _dbContext.Assets.FirstOrDefault(a => a.Id == assetId).CurrentPrice;

                return new Price() { Value = currentPrice };
            }

            var closestPrice = date >= prices.Last().Date
                ? prices.Last()
                : date <= prices.First().Date
                    ? prices.First()
                    : prices.First(p => p.Date >= date);

            return closestPrice;
        }
    }
}
