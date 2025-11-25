using Pazaryeri.Models;

namespace Pazaryeri.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task<Order> CreateAsync(Order siparis);
        Task<Order> UpdateAsync(Order siparis);
        Task DeleteAsync(int id);
        Task<(List<Order> Orders, int TotalCount)> GetPagedOrdersAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Models.Order> OrderExistsAsync(string orderNumber, Platform platform);
        Task<Order> GetWithDetailsAsync(int id);
        Task<TrendyolOrderDetail> GetTrendyolOrderDetailAsync(int orderId);
    }
}
