using AutoMapper;
using InvestmentPortfolioManager.Authorization;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Services
{
    public interface ITransactionService
    {
        public int Create(CreateTransactionDto dto, int walletId);
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

            transaction.WalletId = wallet.Id;

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

        private Wallet GetWalletById(int walletId)
        {
            var wallet = _dbContext.Wallets
                .Include(w => w.User)
                .Include(w=>w.Transactions)
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
    }
}
