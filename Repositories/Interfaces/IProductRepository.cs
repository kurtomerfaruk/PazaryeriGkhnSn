using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IProductRepository:IRepository<Product>
    {
        Task<(List<Product> Products, int TotalCount)> GetPagedProductsAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Product> ProductsExistsAsync(string title, Platform platform);
        Task<Product> GetWithDetailsAsync(int id);
        Task<Product> GetByProductMainIdAsync(string productMainId);
        Task<bool> ProductExistsAsync(string productMainId);

        Task SaveGroup(List<IGrouping<string, TrendyolProductDetail>> Values);
    }
}
