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
        public WalletDto Get(int walletId);
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

        public WalletDto Get(int walletId)
        {
            var wallet = GetWalletById(walletId);
            var positions = GetPositions(wallet.Transactions, wallet.User.Currency);

            var walletDto = _mapper.Map<WalletDto>(wallet);

            walletDto.Positions = positions.ToList();

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
                var walletDto = Get(wallet.Id);
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

        private IEnumerable<Position> GetPositions(List<Transaction> transactions, CurrencyEnum currency)
        {
            var positions = new List<Position>();
            decimal walletValue = 0;

            foreach (var transaction in transactions)
            {
                var position = positions.FirstOrDefault(p => p.AssetId == transaction.AssetId);

                if(position is null)
                {
                    positions.Add(new Position { 
                        AssetId = transaction.AssetId,
                        AssetName = transaction.Asset.Name,
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

                position.CurrentValue = (decimal)position.Quantity * asset.CurrentPrice * currencyAssetPrice;
                position.TotalCost *= currencyAssetPrice;
                position.AvgCost = position.TotalCost / (decimal)position.Quantity;
                position.Profit = position.CurrentValue - position.TotalCost;

                walletValue += position.CurrentValue;
            }

            var updateDate = DateTime.UtcNow;

            foreach(var position in positions)
            {
                position.PercentageInWallet = (double)(position.CurrentValue / walletValue) * 100;
                position.UpdatedDate = updateDate;
            }

            return positions;
        }
        //public class Position
        //{
        //    public int AssetId { get; set; }
        //    public string AssetName { get; set; }
        //    public string Ticker { get; set; }
        //    public double Quantity { get; set; }
        //    public decimal CurrentValue { get; set; }
        //    public decimal TotalCost { get; set; }
        //    public decimal AvgCost { get; set; }
        //    public decimal Profit { get; set; }
        //    public double PercentageInWallet { get; set; }
        //    public DateTime UpdatedDate { get; set; }
        //}
    }
}
