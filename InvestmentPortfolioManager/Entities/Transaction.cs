using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; }
        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public double Quantity { get; set; } // zamienic na decimal i zrobic migracje
        public decimal InitialValue { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
