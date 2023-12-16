namespace InvestmentPortfolioManager.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Name { get; set;}
        public string Details { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<Transaction> Transactions { get; set; }

    }
}
