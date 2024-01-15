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
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InvestmentPortfolioManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            var authenticationSettings = new AuthenticationSettings();
            builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = "Bearer";
                option.DefaultScheme = "Bearer";
                option.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authenticationSettings.JwtIssuer,
                    ValidAudience = authenticationSettings.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
                };
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("UserStatus", builder => builder.RequireClaim("UserStatus", "Ok"));
            });

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddDbContext<InvestmentPortfolioManagerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("InvestmentPortfolioManagerDbConnection")), ServiceLifetime.Transient);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IValidator<CreateWalletDto>, CreateWalletDtoValidator>();
            builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();

            builder.Services.AddScoped<InvestmentPortfolioManagerSeeder>();

            builder.Services.AddScoped<ICoinGeckoAPIService, CoinGeckoAPIService>();
            builder.Services.AddScoped<IBankierScraperService, BankierScraperService>();
            builder.Services.AddScoped<ISlickchartsScraperService, SlickchartsScraperService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<ErrorHandlingMiddleware>();

            builder.Services.AddHostedService<BackgroundWorkerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<InvestmentPortfolioManagerSeeder>();
            seeder.Seed();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investment Portfolio Manager API");
            });

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}