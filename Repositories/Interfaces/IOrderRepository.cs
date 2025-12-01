using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IOrderRepository:IRepository<Order>
    {
        Task<Order> OrderExistsAsync(string orderNumber, Platform platform);
        Task<Order> GetWithDetailsAsync(int id);
        //Task<TrendyolOrderDetail> GetTrendyolOrderDetailAsync(int orderId);
    }
}
