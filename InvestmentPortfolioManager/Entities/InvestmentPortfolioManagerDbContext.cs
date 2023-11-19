using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager.Entities
{
    public class InvestmentPortfolioManagerDbContext : DbContext
    {
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        public InvestmentPortfolioManagerDbContext(DbContextOptions<InvestmentPortfolioManagerDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(eb =>
            {
                eb.Property(a => a.Ticker).IsRequired();
                eb.Property(a => a.Price).IsRequired().HasPrecision(18, 8);
                eb.Property(a => a.Currency).IsRequired();
                eb.Property(a => a.Category).IsRequired();
                eb.Property(a => a.Name).IsRequired();
                eb.Property(a => a.UpdatedDate).IsRequired();
            });

            modelBuilder.Entity<Position>(eb =>
            {
                eb.Property(p => p.Quantity).IsRequired();
                eb.Property(p => p.Status).IsRequired();
                eb.Property(p => p.InitialValue).IsRequired().HasPrecision(18, 8);
                eb.Property(p => p.CreatedDate).IsRequired();
                eb.Property(p => p.UpdatedDate).IsRequired();
            });

            modelBuilder.Entity<User>(eb =>
            {
                eb.Property(u => u.Status).IsRequired();
                eb.Property(u => u.Currency).IsRequired();
                eb.Property(u => u.FirstName).IsRequired();
                eb.Property(u => u.Email).IsRequired();
                eb.Property(u => u.LastName).IsRequired();
                eb.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Wallet>(eb =>
            {
                eb.Property(w => w.CreatedDate).IsRequired();
                eb.Property(w => w.Details).IsRequired();
                eb.Property(w => w.UpdatedDate).IsRequired();
                eb.Property(w => w.Name).IsRequired();
            });
        }
    }
}
