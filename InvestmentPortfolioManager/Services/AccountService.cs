using AutoMapper;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Services
{
   public interface IAccountService
    {
        public void RegisterUser(RegisterUserDto registerUserDto);
        public void Login(LoginDto loginDto);
        public void DeleteUser(int userId);
        public void GetUserById(int userId);
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

        public void Login(LoginDto loginDto)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == loginDto.Email);

            if(user is null)
            {
                throw new BadRequestException("Invalid username or password");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException("Invalid username or password");
            }
        }

        public void GetUserById(int userId)
        {
            var user = _dbContext.Users
                .Include(u=>u.Wallets)
                .FirstOrDefault(u => u.Id == userId);

            if(user is null)
            {
                throw new NotFoundException("User not found");
            }
        }

        public void DeleteUser(int userId)
        {

        }
    }
}
