using AutoMapper;
using InvestmentPortfolioManager.Authorization;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
                throw new BadRequestException("A wallet with this name already exists.");
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
            var walletDto = _mapper.Map<WalletDto>(wallet);

            walletDto.Currency = wallet.User.Currency.ToString();

            if (wallet.Transactions.IsNullOrEmpty())
            {
                walletDto.AssetPositions = new List<AssetPosition>();
                walletDto.AssetCategoryPositions = new List<AssetCategoryPosition>();

                return walletDto;
            }

            var assetPositions = GetAssetPositions(wallet.Transactions, wallet.User.Currency);
            var assetCategoryPositions = GetAssetCategoryPositions(assetPositions);

            walletDto.AssetPositions = assetPositions;
            walletDto.AssetCategoryPositions = assetCategoryPositions;

            decimal sumPricesLast24h = 0, sumPricesLast7d = 0, sumPricesLast1m = 0, sumPricesLast1y = 0;

            foreach (var assetPosition in assetPositions)
            {
                walletDto.CurrentValue += assetPosition.TotalValue;
                walletDto.TotalProfit += assetPosition.Profit;
                walletDto.TotalCost += assetPosition.TotalCost;

                sumPricesLast24h += assetPosition.TotalValue / ((decimal)assetPosition.PercentageChange24h / 100 + 1);
                sumPricesLast7d += assetPosition.TotalValue / ((decimal)assetPosition.PercentageChange7d / 100 + 1);
                sumPricesLast1m += assetPosition.TotalValue / ((decimal)assetPosition.PercentageChange1m / 100 + 1);
                sumPricesLast1y += assetPosition.TotalValue / ((decimal)assetPosition.PercentageChange1y / 100 + 1);
            }

            walletDto.PercentageChange24h = (double)((walletDto.CurrentValue - sumPricesLast24h) / sumPricesLast24h * 100);
            walletDto.PercentageChange7d = (double)((walletDto.CurrentValue - sumPricesLast7d) / sumPricesLast7d * 100);
            walletDto.PercentageChange1m = (double)((walletDto.CurrentValue - sumPricesLast1m) / sumPricesLast1m * 100);
            walletDto.PercentageChange1y = (double)((walletDto.CurrentValue - sumPricesLast1y) / sumPricesLast1y * 100);

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

                position.Price = asset.CurrentPrice * currencyAssetPrice;
                position.TotalValue = position.Quantity * position.Price;
                position.TotalCost *= currencyAssetPrice;
                position.AvgCost = position.TotalCost / position.Quantity;
                position.Profit = position.TotalValue - position.TotalCost;

                var priceLast24h = GetPriceByClosestDate(currentDate.AddHours(-24), asset, currency);
                position.PercentageChange24h = (double)((position.Price - priceLast24h) / priceLast24h * 100);

                var priceLast7d = GetPriceByClosestDate(currentDate.AddDays(-7), asset, currency);
                position.PercentageChange7d = (double)((position.Price - priceLast7d) / priceLast7d * 100);

                var priceLast1m = GetPriceByClosestDate(currentDate.AddMonths(-1), asset, currency);
                position.PercentageChange1m = (double)((position.Price - priceLast1m) / priceLast1m * 100);

                var priceLast1y = GetPriceByClosestDate(currentDate.AddYears(-1), asset, currency);
                position.PercentageChange1y = (double)((position.Price - priceLast1y) / priceLast1y * 100);

                walletValue += position.TotalValue;
            }

            foreach(var position in positions)
            {
                position.PercentageInWallet = (double)(position.TotalValue / walletValue) * 100;
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
                walletValue += position.TotalValue;

                var categoryPosition = categoryPositions.FirstOrDefault(cp => cp.CategoryId == position.AssetCategoryId);

                if(categoryPosition is null)
                {
                    categoryPositions.Add(new AssetCategoryPosition
                    {
                        CategoryId = position.AssetCategoryId,
                        CategoryName = position.AssetCategoryName,
                        TotalValue = position.TotalValue,
                        TotalProfit = position.Profit,
                        TotalCost = position.TotalCost
                    });
                }
                else
                {
                    categoryPosition.TotalValue += position.TotalValue;
                    categoryPosition.TotalProfit += position.Profit;
                    categoryPosition.TotalCost += position.TotalCost;
                }
            }

            foreach(var categoryPosition in categoryPositions)
            {
                categoryPosition.PercentageInWallet = (double)(categoryPosition.TotalValue / walletValue) * 100;

                decimal sumPricesLast24h = 0, sumPricesLast7d = 0, sumPricesLast1m = 0, sumPricesLast1y = 0;

                var p = positions.Where(p => p.AssetCategoryId == categoryPosition.CategoryId);
                
                foreach (var position in p)
                {
                    sumPricesLast24h += position.TotalValue / ((decimal)position.PercentageChange24h / 100 + 1);
                    sumPricesLast7d += position.TotalValue / ((decimal)position.PercentageChange7d / 100 + 1);
                    sumPricesLast1m += position.TotalValue / ((decimal)position.PercentageChange1m / 100 + 1);
                    sumPricesLast1y += position.TotalValue / ((decimal)position.PercentageChange1y / 100 + 1);
                }

                categoryPosition.PercentageChange24h = (double)((categoryPosition.TotalValue - sumPricesLast24h) / sumPricesLast24h * 100);
                categoryPosition.PercentageChange7d = (double)((categoryPosition.TotalValue - sumPricesLast7d) / sumPricesLast7d * 100);
                categoryPosition.PercentageChange1m = (double)((categoryPosition.TotalValue - sumPricesLast1m) / sumPricesLast1m * 100);
                categoryPosition.PercentageChange1y = (double)((categoryPosition.TotalValue - sumPricesLast1y) / sumPricesLast1y * 100);
            }

            return categoryPositions;
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