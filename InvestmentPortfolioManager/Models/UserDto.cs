using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRoleDto Role { get; set; }
        public UserStatusEnum Status { get; set; }
        public CurrencyEnum Currency { get; set; }
        public List<WalletDto> Wallets { get; set; }
    }
}
