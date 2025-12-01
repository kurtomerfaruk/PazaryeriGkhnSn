namespace Pazaryeri.Services
{
    public abstract class BasePlatformService
    {
        protected readonly IConfiguration _configuration;
        protected readonly ILogger _logger;

        public abstract string PlatformName { get; }

        protected BasePlatformService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected virtual string GetOrderStatus(string status)
        {
            return status ?? "Bekliyor";
        }
    }
}
