using AutoMapper;
using InvestmentPortfolioManager.Authorization;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace InvestmentPortfolioManager.Services
{
    public interface ITransactionService
    {
        public int Create(CreateTransactionDto dto, int walletId);
        public void Delete(int walletId, int transactionId);
        public IEnumerable<TransactionDto> GetAllTransactions(int walletId);
        public IEnumerable<TransactionDto> GetTransactionsByAsset(int walletId, int assetId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public TransactionService(InvestmentPortfolioManagerDbContext dbContext, IAuthorizationService authorizationService, IUserContextService userContextService, IMapper mapper)
        {
            _dbContext = dbContext;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public int Create(CreateTransactionDto createTransactionDto, int walletId)
        {
            var wallet = GetWalletById(walletId);
            
            var transaction = _mapper.Map<Transaction>(createTransactionDto);

            if(transaction.Type == TransactionTypeEnum.Sell)
            {
                var transactions = wallet.Transactions.Where(t => t.AssetId == transaction.AssetId);

                decimal sum = 0;

                foreach(var t in transactions)
                {
                    sum += t.Type == TransactionTypeEnum.Sell ? -t.Quantity : t.Quantity;
                }

                if(transaction.Quantity > sum)
                {
                    throw new BadRequestException("Insufficient amount of asset in the wallet.");
                }
            }

            transaction.WalletId = wallet.Id;

            var asset = _dbContext.Assets.FirstOrDefault(a => a.Id == transaction.AssetId);

            if(asset.Currency != wallet.User.Currency)
            {
                var currency = _dbContext.Assets.FirstOrDefault(a => a.Ticker == $"{wallet.User.Currency}/{asset.Currency}");
                var currencyPrice = GetPriceByClosestDate(transaction.TransactionDate, currency, wallet.User.Currency);

                transaction.InitialValue *= currencyPrice;
            }
            
            wallet.UpdatedDate = DateTime.UtcNow;

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            return transaction.Id;
        }

        public void Delete(int walletId, int transactionId)
        {
            var wallet = GetWalletById(walletId);

            var transaction = wallet.Transactions.FirstOrDefault(t => t.Id == transactionId);

            if (transaction is null)
            {
                throw new NotFoundException("Transaction not found.");
            }

            _dbContext.Transactions.Remove(transaction);
            _dbContext.SaveChanges();
        }

        public IEnumerable<TransactionDto> GetAllTransactions(int walletId)
        {
            var wallet = GetWalletById(walletId);
            var transactions = wallet.Transactions.OrderByDescending(t => t.TransactionDate);

            var physicalCurrencyCategoryId = _dbContext.AssetCategories.FirstOrDefault(c => c.Name == AssetCategoryEnum.PhysicalCurrencies.ToString()).Id;
            var currencyAssets = _dbContext.Assets.Where(a => a.CategoryId == physicalCurrencyCategoryId && a.Currency == wallet.User.Currency).ToList();

            foreach (var item in transactions)
            {
                var transactionDto = _mapper.Map<TransactionDto>(item);

                if (item.Asset.Currency != wallet.User.Currency)
                {
                    var currencyAsset = currencyAssets.FirstOrDefault(asset => asset.Ticker == $"{item.Asset.Currency}/{wallet.User.Currency}");

                    transactionDto.InitialValue *= GetPriceByClosestDate(item.TransactionDate, currencyAsset, currencyAsset.Currency);
                }

                transactionDto.Price = transactionDto.InitialValue / transactionDto.Quantity;
                transactionDto.Currency = wallet.User.Currency.ToString();

                yield return transactionDto;
            }
        }

        public IEnumerable<TransactionDto> GetTransactionsByAsset(int walletId, int assetId)
        {
            var wallet = GetWalletById(walletId);

            if(!_dbContext.Assets.Any(a=>a.Id == assetId))
            {
                throw new NotFoundException("Asset not found");
            }

            var transactions = wallet.Transactions.Where(t => t.AssetId == assetId).OrderByDescending(t => t.TransactionDate);

            var physicalCurrencyCategoryId = _dbContext.AssetCategories.FirstOrDefault(c => c.Name == AssetCategoryEnum.PhysicalCurrencies.ToString()).Id;
            var currencyAssets = _dbContext.Assets.Where(a => a.CategoryId == physicalCurrencyCategoryId && a.Currency == wallet.User.Currency).ToList();

            foreach (var item in transactions)
            {
                var transactionDto = _mapper.Map<TransactionDto>(item);

                if (item.Asset.Currency != wallet.User.Currency)
                {
                    var currencyAsset = currencyAssets.FirstOrDefault(asset => asset.Ticker == $"{item.Asset.Currency}/{wallet.User.Currency}");

                    transactionDto.InitialValue *= GetPriceByClosestDate(item.TransactionDate, currencyAsset, currencyAsset.Currency);
                }

                transactionDto.Price = transactionDto.InitialValue / transactionDto.Quantity;
                transactionDto.Currency = wallet.User.Currency.ToString();

                yield return transactionDto;
            }
        }

        private Wallet GetWalletById(int walletId)
        {
            var wallet = _dbContext.Wallets
                .Include(w => w.User)
                .Include(w => w.Transactions)
                .ThenInclude(t => t.Asset)
                .FirstOrDefault(w => w.Id == walletId);

            if(wallet is null)
            {
                throw new NotFoundException("Wallet not found.");
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, wallet.User, new UserResourceRequirement()).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }

            return wallet;
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
