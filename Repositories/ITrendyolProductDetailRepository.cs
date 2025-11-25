using Pazaryeri.Models;

namespace Pazaryeri.Repositories
{
    public interface ITrendyolProductDetailRepository
    {
        Task<TrendyolProductDetail> CreateAsync(TrendyolProductDetail entity);
        Task<TrendyolProductDetail> UpdateAsync(TrendyolProductDetail entity);
        Task<TrendyolProductDetail> GetByTrendyolIdAsync(string productId);
    }
}
