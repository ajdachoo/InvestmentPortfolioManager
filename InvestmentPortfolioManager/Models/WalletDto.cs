using InvestmentPortfolioManager.Entities;

namespace InvestmentPortfolioManager.Models
{
    public class WalletDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public List<Position> Positions { get; set; }
    }
}
