using Microsoft.AspNetCore.Authorization;

namespace InvestmentPortfolioManager.Authorization
{
    public enum ResourceOperation
    {
        Create,
        Read,
        Update,
        Delete
    }

    public class UserResourceRequirement : IAuthorizationRequirement
    {
        public ResourceOperation ResourceOperation { get; }

        public UserResourceRequirement(ResourceOperation resourceOperation)
        {
            ResourceOperation = resourceOperation;
        }
    }
}
