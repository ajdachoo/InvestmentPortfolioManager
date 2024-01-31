using AutoMapper;
using InvestmentPortfolioManager.Authorization;
using InvestmentPortfolioManager.Entities;
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

            var user = _dbContext.Users
                .Include(u => u.Wallets)
                .FirstOrDefault(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, user, new UserResourceRequirement(ResourceOperation.Create)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }

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
                .Include(w=>w.User)
                .FirstOrDefault(w => w.Id == walletId);

            if(wallet is null)
            {
                throw new NotFoundException("Wallet not found.");
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, wallet.User, new UserResourceRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }

            _dbContext.Wallets.Remove(wallet);
            _dbContext.SaveChanges();
        }
    }
}
