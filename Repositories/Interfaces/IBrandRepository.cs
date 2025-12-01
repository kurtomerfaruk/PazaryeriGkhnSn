using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IBrandRepository:IRepository<Brand>
    {
        Task<Brand> BrandExistsAsync(string brandName);
        Task<Brand> GetOrCreateAsync(int trendyolBrandId, string name);
    }
}
