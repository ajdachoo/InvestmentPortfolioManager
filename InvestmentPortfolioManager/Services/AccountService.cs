using AutoMapper;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Identity;

namespace InvestmentPortfolioManager.Services
{
   public interface IAccountService
    {
        public void RegisterUser(RegisterUserDto registerUserDto);
    }

    public class AccountService : IAccountService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountService(InvestmentPortfolioManagerDbContext dbContext, IMapper mapper, IPasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public void RegisterUser(RegisterUserDto registerUserDto)
        {
            var newUser = _mapper.Map<User>(registerUserDto);

            var hashedPassword = _passwordHasher.HashPassword(newUser, registerUserDto.Password);
            newUser.PasswordHash = hashedPassword;

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
        }
    }
}
