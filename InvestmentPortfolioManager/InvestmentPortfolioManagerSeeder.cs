using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager
{
    public class InvestmentPortfolioManagerSeeder
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public InvestmentPortfolioManagerSeeder(InvestmentPortfolioManagerDbContext dbContext, IPasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                var pendingMigrations = _dbContext.Database.GetPendingMigrations();
                if (pendingMigrations != null && pendingMigrations.Any())
                {
                    _dbContext.Database.Migrate();
                }

                if (!_dbContext.AssetCategories.Any())
                {
                    var categories = GetAssetCategories();
                    _dbContext.AssetCategories.AddRange(categories);
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.UserRoles.Any())
                {
                    var roles = GetUserRoles();
                    _dbContext.UserRoles.AddRange(roles);
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.Users.Any())
                {
                    var users = GetUsers();
                    _dbContext.Users.AddRange(users);
                    _dbContext.SaveChanges();
                }
            }
        }

        private IEnumerable<User> GetUsers()
        {
            var userRoleAdminId = _dbContext.UserRoles.FirstOrDefault(r => r.Name == UserRoleEnum.Admin.ToString()).Id;
            var userRoleUserId = _dbContext.UserRoles.FirstOrDefault(r => r.Name == UserRoleEnum.User.ToString()).Id;

            var users = new List<User>()
            {
                new User()
                {
                    Currency = CurrencyEnum.USD,
                    Email = "jannowak@gmail.com",
                    FirstName = "Jan",
                    LastName = "Nowak",
                    RoleId = userRoleAdminId,
                    Status = UserStatusEnum.Ok,
                },
                new User()
                {
                    Currency = CurrencyEnum.PLN,
                    Email = "krzysztofkowalski@gmail.com",
                    FirstName = "Krzysztof",
                    LastName = "Kowalski",
                    RoleId = userRoleUserId,
                    Status = UserStatusEnum.Ok,
                }
            };

            users.ElementAt(0).PasswordHash = _passwordHasher.HashPassword(users.ElementAt(0), "jannowak");
            users.ElementAt(1).PasswordHash = _passwordHasher.HashPassword(users.ElementAt(1), "krzysztofkowalski");

            return users;
        }

        private IEnumerable<UserRole> GetUserRoles()
        {
            var roles = new List<UserRole>()
            {
                new UserRole()
                {
                    Name = UserRoleEnum.Admin.ToString(),
                },
                new UserRole()
                {
                    Name = UserRoleEnum.User.ToString(),
                }
            };

            return roles;
        }

        private IEnumerable<AssetCategory> GetAssetCategories()
        {
            var categories = new List<AssetCategory>()
            {
                new AssetCategory()
                {
                    Name = AssetCategoryEnum.Cryptocurrencies.ToString(),
                },
                new AssetCategory()
                {
                    Name = AssetCategoryEnum.PhysicalCurrencies.ToString(),
                },
                new AssetCategory()
                {
                    Name = AssetCategoryEnum.PolishStocks.ToString(),
                },
                new AssetCategory()
                {
                    Name = AssetCategoryEnum.USStocks.ToString(),
                }
            };

            return categories;
        }
    }
}
