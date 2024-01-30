using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InvestmentPortfolioManager.Authorization
{
    public class UserResourceRequirementHandler : AuthorizationHandler<UserResourceRequirement, User>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserResourceRequirement requirement, User user)
        {
            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;
            
            if(user.Id == int.Parse(userId))
            {
                context.Succeed(requirement);
            }

            if (context.User.IsInRole(UserRoleEnum.Admin.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
