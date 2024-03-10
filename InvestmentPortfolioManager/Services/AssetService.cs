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

            foreach (var asset in assets)
            {
                if(asset.CategoryId == physicalCurrencyCategoryId && asset.Currency != currencyEnum)
                {
                    continue;
                }

                if(asset.Currency != currencyEnum)
                {
                    var newCurrency = currencies.FirstOrDefault(a => a.Ticker == $"{asset.Currency}/{currency}");

                    asset.Price *= newCurrency.Price;
                    asset.Currency = currencyEnum;
                }

                var assetDto = _mapper.Map<AssetDto>(asset);

                yield return assetDto;
            }
        }
    }
}
