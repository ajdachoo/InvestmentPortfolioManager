using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public virtual int RoleId { get; set; }
        public UserStatusEnum Status { get; set; }
        public CurrencyEnum Currency { get; set; }
        public virtual List<Wallet> Wallets { get; set; }
    }
}
