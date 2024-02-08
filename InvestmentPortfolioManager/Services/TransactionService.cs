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

                double sum = 0;

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
            wallet.UpdatedDate = DateTime.UtcNow;

            _dbContext.Transactions.Add(transaction);
            _dbContext.Wallets.Update(wallet);
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

            var transactionDto = _mapper.Map<IEnumerable<TransactionDto>>(wallet.Transactions);

            return transactionDto;            
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
    }
}
