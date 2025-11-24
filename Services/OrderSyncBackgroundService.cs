namespace Pazaryeri.Services
{
    public class OrderSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderSyncBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public OrderSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OrderSyncBackgroundService> logger,
        IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _configuration.GetValue<int>("SyncSettings:IntervalMinutes", 5);
            var isEnabled = _configuration.GetValue<bool>("SyncSettings:Enabled", true);
            var syncInterval = TimeSpan.FromMinutes(intervalMinutes);

            if (!isEnabled)
            {
                _logger.LogInformation("Sipariş senkronizasyon servisi devre dışı bırakıldı");
                return;
            }

            _logger.LogInformation("Sipariş Senkronizasyon Servisi başlatıldı. Interval: {Interval} dakika", intervalMinutes);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderSyncService = scope.ServiceProvider.GetRequiredService<IOrderSyncService>();
                        await orderSyncService.SyncOrdersAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sipariş senkronizasyonu sırasında hata oluştu");
                }

                await Task.Delay(syncInterval, stoppingToken);
            }
        }
    }
}
