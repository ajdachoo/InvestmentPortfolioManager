using FluentValidation;
using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Enums;
using System.Globalization;

namespace InvestmentPortfolioManager.Models.Validators
{
    public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
    {
        public CreateTransactionDtoValidator(InvestmentPortfolioManagerDbContext dbContext)
        {
            RuleFor(x => x.InitialValue).NotEmpty().GreaterThan(0);

            RuleFor(x => x.Quantity).NotEmpty().GreaterThan(0);

            RuleFor(x => x.Type).NotEmpty().IsEnumName(typeof(TransactionTypeEnum), false);

            RuleFor(x => x.AssetId).NotEmpty().Custom((value, context) =>
            {
                var isAssetExist = dbContext.Assets.Any(a => a.Id == value);

                if (!isAssetExist)
                {
                    context.AddFailure("AssetId", "Asset not found");
                }
            });

            RuleFor(x => x.TransactionDate).Custom((value, context) =>
            {
                var isValidDateFormat = DateTime.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss.fffZ", null, DateTimeStyles.AdjustToUniversal, out _);

                if (!isValidDateFormat && !String.IsNullOrEmpty(value))
                {
                    context.AddFailure("TransactionDate", "Invalid date format, required date format is ISO8601.");
                }
            });
        }
    }
}
