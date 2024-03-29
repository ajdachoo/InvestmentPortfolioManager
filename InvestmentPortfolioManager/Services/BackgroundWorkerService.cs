﻿namespace InvestmentPortfolioManager.Services
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BackgroundWorkerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cryptocurrencyAPIService = scope.ServiceProvider.GetRequiredService<ICoinGeckoAPIService>();
                var bankierScraperService = scope.ServiceProvider.GetService<IBankierScraperService>();
                var slickchartsScraperService = scope.ServiceProvider.GetService<ISlickchartsScraperService>();

                await Task.WhenAll(
                    cryptocurrencyAPIService.UpdateAssets(stoppingToken),
                    bankierScraperService.UpdateStockAssets(stoppingToken),
                    bankierScraperService.UpdateForexAssets(stoppingToken),
                    slickchartsScraperService.UpdateAssets(stoppingToken));
            }
        }
    }
}
