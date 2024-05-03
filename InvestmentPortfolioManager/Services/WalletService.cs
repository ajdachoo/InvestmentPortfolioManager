using AutoMapper;
using InvestmentPortfolioManager.Authorization;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Services
{
    public interface IWalletService
    {
        public int Create(CreateWalletDto createWalletDto, int? userId);
        public void Delete(int walletId);
        public WalletDto GetWallet(int walletId);
        public IEnumerable<WalletDto> GetAll(int? userId);
    }

    public class WalletService : IWalletService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly IUserContextService _userContextService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        public WalletService(InvestmentPortfolioManagerDbContext dbContext, IUserContextService userContextService, IAuthorizationService authorizationService, IMapper mapper)
        {
            _dbContext = dbContext;
            _userContextService = userContextService;
            _authorizationService = authorizationService;
            _mapper = mapper;
        }
  
        public int Create(CreateWalletDto createWalletDto, int? userId)
        {
            if(userId is null)
            {
                userId = _userContextService.GetUserId;
            }

            var user = GetUserById((int)userId);

            if(user.Wallets.Any(w=>w.Name == createWalletDto.Name))
            {
                throw new BadRequestException("A wallet with this name already exist.");
            }

            var updateDate = DateTime.UtcNow;

            var wallet = _mapper.Map<Wallet>(createWalletDto);

            wallet.UpdatedDate = updateDate;
            wallet.CreatedDate = updateDate;
            wallet.UserId = (int)userId;

            _dbContext.Wallets.Add(wallet);
            _dbContext.SaveChanges();

            return wallet.Id;
        }

        public void Delete(int walletId)
        {
            var wallet = _dbContext.Wallets
                .Include(w => w.User)
                .FirstOrDefault(w => w.Id == walletId);

            if(wallet is null)
            {
                throw new NotFoundException("Wallet not found.");
            }

            AuthorizeUser(wallet.User);

            _dbContext.Wallets.Remove(wallet);
            _dbContext.SaveChanges();
        }

        public WalletDto GetWallet(int walletId)
        {
            var wallet = GetWalletById(walletId);
            var assetPositions = GetAssetPositions(wallet.Transactions, wallet.User.Currency);
            var assetCategoryPositions = GetAssetCategoryPositions(assetPositions);

            var walletDto = _mapper.Map<WalletDto>(wallet);

            walletDto.AssetPositions = assetPositions;
            walletDto.Currency = wallet.User.Currency.ToString();
            walletDto.CurrentValue = assetPositions.Sum(p => p.CurrentValue);
            walletDto.TotalProfit = assetPositions.Sum(p => p.Profit);
            walletDto.TotalCost = assetPositions.Sum(p => p.TotalCost);
            walletDto.AssetCategoryPositions = assetCategoryPositions;

            return walletDto;
        }

        public IEnumerable<WalletDto> GetAll(int? userId)
        {
            if(userId is null)
            {
                userId = _userContextService.GetUserId;
            }

            var user = GetUserById((int)userId);

            foreach(var wallet in user.Wallets)
            {
                var walletDto = GetWallet(wallet.Id);
                yield return walletDto;
            }
        } 

        private User GetUserById(int userId)
        {
            var user = _dbContext.Users
                .Include(u => u.Wallets)
                .FirstOrDefault(u => u.Id == userId);

            if(user is null)
            {
                throw new NotFoundException("User not found.");
            }

            AuthorizeUser(user);

            return user;
        }

        private Wallet GetWalletById(int walletId)
        {
            var wallet = _dbContext.Wallets
                .Include(w => w.User)
                .Include(w => w.Transactions)
                .ThenInclude(t => t.Asset)
                .ThenInclude(a => a.Category)
                .FirstOrDefault(w => w.Id == walletId);

            if (wallet is null)
            {
                throw new NotFoundException("Wallet not found.");
            }

            AuthorizeUser(wallet.User);

            return wallet;
        }

        private void AuthorizeUser(User user)
        {
            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, user, new UserResourceRequirement()).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }
        }

        private void AuthorizeUser(int userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if(user is null)
            {
                throw new NotFoundException("User not found.");
            }

            AuthorizeUser(user);
        }

        private List<AssetPosition> GetAssetPositions(List<Transaction> transactions, CurrencyEnum currency)
        {
            var positions = new List<AssetPosition>();
            decimal walletValue = 0;
            var currentDate = DateTime.UtcNow;

            foreach (var transaction in transactions)
            {
                var position = positions.FirstOrDefault(p => p.AssetId == transaction.AssetId);

                if(position is null)
                {
                    positions.Add(new AssetPosition { 
                        AssetId = transaction.AssetId,
                        AssetName = transaction.Asset.Name,
                        AssetCategoryName = transaction.Asset.Category.Name,
                        AssetCategoryId = transaction.Asset.Category.Id,
                        Ticker = transaction.Asset.Ticker,
                        Quantity = transaction.Quantity,
                        TotalCost = transaction.InitialValue,
                    });
                }
                else
                {
                    position.Quantity += transaction.Quantity;
                    position.TotalCost += transaction.InitialValue;
                }
            }

            foreach(var position in positions)
            {
                var asset = _dbContext.Assets.FirstOrDefault(a => a.Id == position.AssetId);
                decimal currencyAssetPrice = 1;

                if(asset is null)
                {
                    throw new NotFoundException("Asset not found.");
                }

                if(asset.Currency != currency)
                {
                    var currencyAsset = _dbContext.Assets.FirstOrDefault(a => a.Ticker == $"{asset.Currency}/{currency}");

                    if (currencyAsset is null)
                    {
                        throw new NotFoundException("Currency not found.");
                    }

                    currencyAssetPrice = currencyAsset.CurrentPrice;
                }

                var assetFinalPrice = asset.CurrentPrice * currencyAssetPrice;

                position.CurrentValue = position.Quantity * assetFinalPrice;
                position.TotalCost *= currencyAssetPrice;
                position.AvgCost = position.TotalCost / position.Quantity;
                position.Profit = position.CurrentValue - position.TotalCost;

                position.PercentageChange24h = (double)(((assetFinalPrice - GetPriceByClosestDate(currentDate.AddHours(-24), asset, currency)) / assetFinalPrice) * 100);
                position.PercentageChange7d = (double)(((assetFinalPrice - GetPriceByClosestDate(currentDate.AddDays(-7), asset, currency)) / assetFinalPrice) * 100);
                position.PercentageChange1m = (double)(((assetFinalPrice - GetPriceByClosestDate(currentDate.AddMonths(-1), asset, currency)) / assetFinalPrice) * 100);
                position.PercentageChange1y = (double)(((assetFinalPrice - GetPriceByClosestDate(currentDate.AddYears(-1), asset, currency)) / assetFinalPrice) * 100);

                walletValue += position.CurrentValue;
            }

            foreach(var position in positions)
            {
                position.PercentageInWallet = (double)(position.CurrentValue / walletValue) * 100;
                position.UpdatedDate = currentDate;
            }

            return positions;
        }

        private List<AssetCategoryPosition> GetAssetCategoryPositions(List<AssetPosition> positions)
        {
            decimal walletValue = 0;
            var categoryPositions = new List<AssetCategoryPosition>();

            foreach (var position in positions)
            {
                walletValue += position.CurrentValue;

                var categoryPosition = categoryPositions.FirstOrDefault(cp => cp.CategoryId == position.AssetCategoryId);

                if(categoryPosition is null)
                {
                    categoryPositions.Add(new AssetCategoryPosition
                    {
                        CategoryId = position.AssetCategoryId,
                        CategoryName = position.AssetCategoryName,
                        TotalValue = position.CurrentValue,
                        TotalProfit = position.Profit,
                        TotalCost = position.TotalCost
                    });
                }
                else
                {
                    categoryPosition.TotalValue += position.CurrentValue;
                    categoryPosition.TotalProfit += position.Profit;
                    categoryPosition.TotalCost += position.TotalCost;
                }
            }

            foreach(var categoryPosition in categoryPositions)
            {
                categoryPosition.PercentageInWallet = (double)(categoryPosition.TotalValue / walletValue) * 100;
            }

            return categoryPositions;
        }

        private decimal GetPriceByClosestDate(DateTime date, Asset asset, CurrencyEnum currency)
        {
            decimal currencyPrice = 1;

            var assetPrices = _dbContext.Prices.Where(p => p.AssetId == asset.Id).OrderBy(p => p.Date).ToList();

            if (asset.Currency != currency)
            {
                var currencyAsset = _dbContext.Assets.FirstOrDefault(a => a.Ticker == $"{asset.Currency}/{currency}");
                var currencyAssetPrices = _dbContext.Prices.Where(p => p.AssetId == currencyAsset.Id).OrderBy(p => p.Date).ToList();

                if (currencyAssetPrices.Count > 0)
                {
                    var closestCurrencyAssetPrice = date >= currencyAssetPrices.Last().Date
                        ? currencyAssetPrices.Last()
                        : date <= currencyAssetPrices.First().Date
                            ? currencyAssetPrices.First()
                            : currencyAssetPrices.First(p => p.Date >= date);

                    currencyPrice = closestCurrencyAssetPrice.Value;
                }
                else
                {
                    currencyPrice = currencyAsset.CurrentPrice;
                }
            }

            if (assetPrices.Count == 0)
            {
                return asset.CurrentPrice * currencyPrice;
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