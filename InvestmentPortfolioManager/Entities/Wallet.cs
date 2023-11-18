namespace InvestmentPortfolioManager.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Name { get; set;}
        public string Details { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int OwnerId { get; set; }
        public virtual User Owner { get; set; }
        public virtual List<Position> Positions { get; set; }

    }
}
