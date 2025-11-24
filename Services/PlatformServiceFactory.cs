namespace Pazaryeri.Services
{
    public class PlatformServiceFactory : IPlatformServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public PlatformServiceFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public IPlatformService GetService(string platformName)
        {
            return platformName.ToLower() switch
            {
                "trendyol" => _serviceProvider.GetRequiredService<TrendyolService>(),
                _ => throw new ArgumentException($"Bilinmeyen platform: {platformName}")
            };
        }

        public List<string> GetAvailablePlatforms()
        {
            return new List<string> { "Trendyol" };
        }
    }
}
