using Pazaryeri.Models;

namespace Pazaryeri.Services
{
    public interface IPlatformService
    {
        Task<List<Order>> GetOrdersAsync();
        string PlatformName { get; }
    }
}
