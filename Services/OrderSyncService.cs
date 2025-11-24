using Pazaryeri.Repositories;

namespace Pazaryeri.Services
{
    public interface IOrderSyncService
    {
        Task SyncOrdersAsync();
    }
    public class OrderSyncService : IOrderSyncService
    {
        private readonly IPlatformServiceFactory _platformServiceFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderSyncService> _logger;

        public OrderSyncService(
            IPlatformServiceFactory platformServiceFactory,
            IOrderRepository orderRepository,
            ILogger<OrderSyncService> logger)
        {
            _platformServiceFactory = platformServiceFactory;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task SyncOrdersAsync()
        {
            _logger.LogInformation("Otomatik sipariş senkronizasyonu başladı: {Time}", DateTime.Now);

            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            var results = new List<PlatformSyncResult>();

            foreach (var platform in platforms)
            {
                try
                {
                    _logger.LogInformation("{Platform} siparişleri çekiliyor...", platform);

                    var platformService = _platformServiceFactory.GetService(platform);
                    var orders = await platformService.GetOrdersAsync();

                    int addedCount = 0;
                    var errors = new List<string>();

                    foreach (var order in orders)
                    {
                        try
                        {
                            if (!await _orderRepository.OrderExistsAsync(order.OrderNumber, order.Platform))
                            {
                                await _orderRepository.CreateAsync(order);
                                addedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Sipariş {order.OrderNumber}: {ex.Message}");
                            _logger.LogWarning(ex, "Sipariş kaydedilemedi: {OrderNumber}", order.OrderNumber);
                        }
                    }

                    results.Add(new PlatformSyncResult
                    {
                        Platform = platform,
                        TotalFetched = orders.Count,
                        AddedCount = addedCount,
                        Errors = errors
                    });

                    _logger.LogInformation("{Platform}: {Added}/{Total} yeni sipariş eklendi",
                        platform, addedCount, orders.Count);

                    // Platformlar arası küçük bekleme
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Platform} siparişleri çekilirken hata oluştu", platform);
                    results.Add(new PlatformSyncResult
                    {
                        Platform = platform,
                        TotalFetched = 0,
                        AddedCount = 0,
                        Errors = new List<string> { ex.Message }
                    });
                }
            }

            // Sonuçları logla
            LogSyncResults(results);
        }

        private void LogSyncResults(List<PlatformSyncResult> results)
        {
            var totalAdded = results.Sum(r => r.AddedCount);
            var totalFetched = results.Sum(r => r.TotalFetched);
            var totalErrors = results.Sum(r => r.Errors.Count);

            _logger.LogInformation("Senkronizasyon tamamlandı: {Added} yeni sipariş, {Fetched} çekilen, {Errors} hata",
                totalAdded, totalFetched, totalErrors);

            foreach (var result in results)
            {
                _logger.LogInformation("{Platform}: {Added}/{Fetched} eklendi, {Errors} hata",
                    result.Platform, result.AddedCount, result.TotalFetched, result.Errors.Count);
            }
        }
    }

    public class PlatformSyncResult
    {
        public string Platform { get; set; }
        public int TotalFetched { get; set; }
        public int AddedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
