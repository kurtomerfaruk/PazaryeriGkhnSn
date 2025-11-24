namespace Pazaryeri.Services
{
    public class PlatformServiceFactory : IPlatformServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public PlatformServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TrendyolService GetTrendyolService()
        {
            return _serviceProvider.GetRequiredService<TrendyolService>();
        }

      
        public List<string> GetAvailablePlatforms()
        {
            return new List<string> { "Trendyol" };
        }
    }
}
