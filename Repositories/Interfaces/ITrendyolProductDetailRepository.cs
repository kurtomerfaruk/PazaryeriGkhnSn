using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface ITrendyolProductDetailRepository
    {
        Task<TrendyolProductDetail> CreateAsync(TrendyolProductDetail entity);
        Task<TrendyolProductDetail> UpdateAsync(TrendyolProductDetail entity);
        Task<TrendyolProductDetail> GetByTrendyolIdAsync(string productId);
    }
}
