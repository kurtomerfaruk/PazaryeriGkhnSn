namespace Pazaryeri.Services
{
    public interface IPlatformServiceFactory
    {
        TrendyolService GetTrendyolService();
        List<string> GetAvailablePlatforms();
    }
}
