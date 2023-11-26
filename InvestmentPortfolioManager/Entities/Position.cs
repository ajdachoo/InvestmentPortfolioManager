using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Entities
{
    public class Position
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; }
        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }
        public PositionStatusEnum Status { get; set; }
        public double Quantity { get; set; }
        public decimal InitialValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
