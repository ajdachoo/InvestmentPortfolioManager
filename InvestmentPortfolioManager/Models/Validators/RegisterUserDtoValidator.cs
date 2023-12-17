using FluentValidation;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;

namespace InvestmentPortfolioManager.Models.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator(InvestmentPortfolioManagerDbContext dbContext)
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().Custom((value, context) =>
            {
                var isEmailExist = dbContext.Users.Any(u => u.Email == value.ToLower());
                
                if (isEmailExist)
                {
                    context.AddFailure("Email", "That email is taken.");
                }
            });

            RuleFor(x => x.Currency).NotEmpty().IsEnumName(typeof(CurrencyEnum), false);

            RuleFor(x => x.Password).MinimumLength(6);

            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);

            RuleFor(x => x.FirstName).NotEmpty();

            RuleFor(x => x.LastName).NotEmpty();
        }
    }
}
