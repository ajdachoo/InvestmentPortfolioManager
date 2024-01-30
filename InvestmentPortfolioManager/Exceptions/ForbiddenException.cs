namespace InvestmentPortfolioManager.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "You don't have permission to acces.") : base(message)
        {
            
        }
    }
}
