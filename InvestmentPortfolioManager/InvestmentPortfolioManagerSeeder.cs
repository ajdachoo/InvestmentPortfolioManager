using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager
{
    public class InvestmentPortfolioManagerSeeder
    {
        private readonly InvestmentPortfolioManagerDbContext _dbContext;

        public InvestmentPortfolioManagerSeeder(InvestmentPortfolioManagerDbContext dbContext)
        {
            _dbContext = dbContext;
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
            }
        }

        private IEnumerable<UserRole> GetUserRoles()
        {
            var roles = new List<UserRole>()
            {
                new UserRole()
                {
                    Name = "admin",
                },
                new UserRole()
                {
                    Name = "user"
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
