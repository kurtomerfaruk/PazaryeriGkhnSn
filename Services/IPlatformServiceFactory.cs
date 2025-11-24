namespace Pazaryeri.Services
{
    public interface IPlatformServiceFactory
    {
        IPlatformService GetService(string platformName);
        List<string> GetAvailablePlatforms();
    }
}
