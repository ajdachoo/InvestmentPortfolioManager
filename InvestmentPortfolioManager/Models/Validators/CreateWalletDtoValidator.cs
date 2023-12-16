using FluentValidation;
using InvestmentPortfolioManager.Entities;
using System.Globalization;

namespace InvestmentPortfolioManager.Models.Validators
{
    public class CreateWalletDtoValidator : AbstractValidator<CreateWalletDto>
    {
        public CreateWalletDtoValidator(InvestmentPortfolioManagerDbContext dbContext)
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.CreatedDate).NotEmpty().Custom((value, context) =>
            {
                var isValidDateFormat = DateTime.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss.fffZ", null, DateTimeStyles.AdjustToUniversal, out _);

                if (!isValidDateFormat)
                {
                    context.AddFailure("CreatedDate", "Invalid date format, required date format is ISO8601.");
                }
            });

            RuleFor(x => x.UserId).NotEmpty().Custom((value, context) =>
            {
                var isUserExist = dbContext.Users.Any(u => u.Id == value);

                if (!isUserExist)
                {
                    context.AddFailure("UserId", $"User with id: {value} not found.");
                }
            });

            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
