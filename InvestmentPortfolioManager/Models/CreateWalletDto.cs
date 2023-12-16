using InvestmentPortfolioManager.Entities;

namespace InvestmentPortfolioManager.Models
{
    public class CreateWalletDto
    {
        public string Name { get; set; }
        public string Details { get; set; }
        public string CreatedDate { get; set; }
        public int UserId { get; set; }
    }
}
