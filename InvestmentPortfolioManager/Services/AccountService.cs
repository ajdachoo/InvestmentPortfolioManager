using AutoMapper;
using InvestmentPortfolioManager.Authorization;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using InvestmentPortfolioManager.Exceptions;
using InvestmentPortfolioManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvestmentPortfolioManager.Services
{
   public interface IAccountService
    {
        public void RegisterUser(RegisterUserDto registerUserDto);
        public string GenerateJwt(LoginDto loginDto);
        public void DeleteUser(int? userId);
        public UserDto GetUserById(int? userId);
        public void UpdateUser(int? userId);
    }

    public class AccountService : IAccountService
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IUserContextService _userContextService;
        private readonly IAuthorizationService _authorizationService;

        public AccountService(InvestmentPortfolioManagerDbContext dbContext, IMapper mapper, IPasswordHasher<User> passwordHasher, AuthenticationSettings authenticationSettings, IUserContextService userContextService, IAuthorizationService authorizationService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
            _userContextService = userContextService;
            _authorizationService = authorizationService;
        }

        public void RegisterUser(RegisterUserDto registerUserDto)
        {
            var newUser = _mapper.Map<User>(registerUserDto);

            var role = _dbContext.UserRoles.FirstOrDefault(r => r.Name == UserRoleEnum.User.ToString());

            if(role is null)
            {
                throw new NotFoundException($"Role \"{UserRoleEnum.User}\" does not exist");
            }

            var hashedPassword = _passwordHasher.HashPassword(newUser, registerUserDto.Password);
            newUser.PasswordHash = hashedPassword;
            newUser.RoleId = role.Id;

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
        }

        public string GenerateJwt(LoginDto loginDto)
        {
            var user = _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == loginDto.Email);

            if (user is null)
            {
                throw new BadRequestException("Invalid username or password");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException("Invalid username or password");
            }

            if(user.Status == UserStatusEnum.Blocked)
            {
                throw new ForbiddenException("Account has been blocked");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, $"{user.Role.Name}"),
                new Claim("RoleId", $"{user.Role.Id}"),
                new Claim("Status", user.Status.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);

        }

        public UserDto GetUserById(int? userId)
        {
            if (userId is null)
            {
                userId = _userContextService.GetUserId;
            }

            var user = _dbContext.Users
                .Include(u=>u.Wallets)
                .Include(u=>u.Role)
                .FirstOrDefault(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, user, new UserResourceRequirement()).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }

            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
        }

        public void DeleteUser(int? userId)
        {
            if(userId is null)
            {
                userId = _userContextService.GetUserId;
            }
            
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, user, new UserResourceRequirement()).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbiddenException();
            }

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }

        public void UpdateUser(int? userId)
        {
            
        }
    }
}
