using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Middleware;
using InvestmentPortfolioManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using InvestmentPortfolioManager.Models;
using InvestmentPortfolioManager.Models.Validators;

namespace InvestmentPortfolioManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddDbContext<InvestmentPortfolioManagerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("InvestmentPortfolioManagerDbConnection")), ServiceLifetime.Transient);

            builder.Services.AddScoped<IValidator<CreateWalletDto>, CreateWalletDtoValidator>();
            builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();

            builder.Services.AddScoped<ICoinGeckoAPIService, CoinGeckoAPIService>();
            builder.Services.AddScoped<IBankierScraperService, BankierScraperService>();
            builder.Services.AddScoped<ISlickchartsScraperService, SlickchartsScraperService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<ErrorHandlingMiddleware>();

            builder.Services.AddHostedService<BackgroundWorkerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}