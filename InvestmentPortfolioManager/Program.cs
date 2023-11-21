using InvestmentPortfolioManager.Entities;
using InvestmentPortfolioManager.Services;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<InvestmentPortfolioManagerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("InvestmentPortfolioManagerDbConnection")));

            builder.Services.AddScoped<ICryptocurrencyAPIService, CryptocurrencyAPIService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}