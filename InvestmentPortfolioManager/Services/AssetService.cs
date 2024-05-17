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

                var assetDto = _mapper.Map<AssetDto>(asset);

                decimal currencyAssetPrice = 1;

                if (asset.Currency != currencyEnum)
                {
                    var newCurrency = currencies.FirstOrDefault(a => a.Ticker == $"{asset.Currency}/{currency}");

                    currencyAssetPrice = newCurrency.CurrentPrice;
                    assetDto.Currency = currencyEnum.ToString();
                }
                
                assetDto.CurrentPrice *= currencyAssetPrice;

                var priceLast24h = GetPriceByClosestDate(currentDate.AddHours(-24), asset, currencyEnum);
                assetDto.PercentageChange24h = (double)((assetDto.CurrentPrice - priceLast24h) / priceLast24h * 100);

                var priceLast7d = GetPriceByClosestDate(currentDate.AddDays(-7), asset, currencyEnum);
                assetDto.PercentageChange7d = (double)((assetDto.CurrentPrice - priceLast7d) / priceLast7d * 100);

                var priceLast1m = GetPriceByClosestDate(currentDate.AddMonths(-1), asset, currencyEnum);
                assetDto.PercentageChange1m = (double)((assetDto.CurrentPrice - priceLast1m) / priceLast1m * 100);

                var priceLast1y = GetPriceByClosestDate(currentDate.AddYears(-1), asset, currencyEnum);
                assetDto.PercentageChange1y = (double)((assetDto.CurrentPrice - priceLast1y) / priceLast1y * 100);

                yield return assetDto;
            }
        }

        private decimal GetPriceByClosestDate(DateTime date, Asset asset, CurrencyEnum currency)
        {
            decimal currencyPrice = 1;

            var assetPrices = _dbContext.Prices.Where(p => p.AssetId == asset.Id).OrderBy(p => p.Date).ToList();
            assetPrices.Add(new Price { AssetId = asset.Id, Date = asset.UpdatedDate, Value = asset.CurrentPrice });

            if (asset.Currency != currency)
            {
                var currencyAsset = _dbContext.Assets.FirstOrDefault(a => a.Ticker == $"{asset.Currency}/{currency}");
                var currencyAssetPrices = _dbContext.Prices.Where(p => p.AssetId == currencyAsset.Id).OrderBy(p => p.Date).ToList();
                currencyAssetPrices.Add(new Price { Date = currencyAsset.UpdatedDate, AssetId = currencyAsset.Id, Value = currencyAsset.CurrentPrice });

                var closestCurrencyAssetPrice = date >= currencyAssetPrices.Last().Date
                        ? currencyAssetPrices.Last()
                        : date <= currencyAssetPrices.First().Date
                            ? currencyAssetPrices.First()
                            : currencyAssetPrices.First(p => p.Date >= date);

                currencyPrice = closestCurrencyAssetPrice.Value;
            }

            var closestAssetPrice = date >= assetPrices.Last().Date
                ? assetPrices.Last()
                : date <= assetPrices.First().Date
                    ? assetPrices.First()
                    : assetPrices.First(p => p.Date >= date);

            return closestAssetPrice.Value * currencyPrice;
        }
    }
}
