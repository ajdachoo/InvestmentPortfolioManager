namespace InvestmentPortfolioManager.Services
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
                var cryptocurrencyAPIService = scope.ServiceProvider.GetRequiredService<ICryptocurrencyAPIService>();

                await cryptocurrencyAPIService.UpdateAssets(stoppingToken);
            }
        }
    }
}
