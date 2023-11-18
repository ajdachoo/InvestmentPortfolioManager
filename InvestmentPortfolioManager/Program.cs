using InvestmentPortfolioManager.Entities;
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

            builder.Services.AddDbContext<InvestmentPortfolioManagerDbContext>(options => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InvestmentPortfolioManagerDb;Trusted_Connection=True;"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}