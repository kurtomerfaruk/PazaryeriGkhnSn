using Pazaryeri.Models;

namespace Pazaryeri.Repositories
{
    public interface IBrandRepository:IRepository<Brand>
    {
        Task<(List<Brand> Brands, int TotalCount)> GetPagedBrandsAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Brand> BrandExistsAsync(string brandName);
    }
}
