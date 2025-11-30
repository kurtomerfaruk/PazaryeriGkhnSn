using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IBrandRepository:IRepository<Brand>
    {
        Task<(List<Brand> Brands, int TotalCount)> GetPagedBrandsAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Brand> BrandExistsAsync(string brandName);
        Task<Brand> GetOrCreateAsync(int trendyolBrandId, string name);
    }
}
