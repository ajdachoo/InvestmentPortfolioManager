using InvestmentPortfolioManager.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Entities
{
    public class InvestmentPortfolioManagerDbContext : DbContext
    {
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        public InvestmentPortfolioManagerDbContext(DbContextOptions<InvestmentPortfolioManagerDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(eb =>
            {
                eb.Property(a => a.Ticker).IsRequired();
                eb.Property(a => a.CurrentPrice).IsRequired().HasPrecision(18, 8);
                eb.Property(a => a.Currency).IsRequired().HasConversion<string>();
                eb.Property(a => a.CategoryId).IsRequired();
                eb.Property(a => a.Name).IsRequired();
                eb.Property(a => a.UpdatedDate).IsRequired();
            });

            modelBuilder.Entity<AssetCategory>(eb =>
            {
                eb.Property(a => a.Name).IsRequired();
            });

            modelBuilder.Entity<Price>(eb =>
            {
                eb.Property(a => a.AssetId).IsRequired();
                eb.Property(a => a.Date).IsRequired();
                eb.Property(a => a.Value).IsRequired().HasPrecision(18, 8);
            });

            modelBuilder.Entity<Transaction>(eb =>
            {
                eb.Property(p => p.Quantity).IsRequired().HasPrecision(18, 8);
                eb.Property(p => p.Type).IsRequired().HasConversion<string>();
                eb.Property(p => p.InitialValue).IsRequired().HasPrecision(18, 8);
                eb.Property(p => p.TransactionDate).IsRequired();
                eb.Property(p => p.WalletId).IsRequired();
                eb.Property(p => p.AssetId).IsRequired();
            });

            modelBuilder.Entity<User>(eb =>
            {
                eb.Property(u => u.Status).IsRequired().HasConversion<string>();
                eb.Property(u => u.RoleId).IsRequired();
                eb.Property(u => u.Currency).IsRequired().HasConversion<string>();
                eb.Property(u => u.FirstName).IsRequired();
                eb.Property(u => u.Email).IsRequired();
                eb.Property(u => u.LastName).IsRequired();
                eb.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<UserRole>(eb =>
            {
                eb.Property(u => u.Name).IsRequired();
            });

            modelBuilder.Entity<Wallet>(eb =>
            {
                eb.Property(w => w.CreatedDate).IsRequired();
                eb.Property(w => w.Details).IsRequired();
                eb.Property(w => w.UpdatedDate).IsRequired();
                eb.Property(w => w.Name).IsRequired();
                eb.Property(w => w.UserId).IsRequired();
            });
        }
    }
}
