using Pazaryeri.Entity.Trendyol.Products;
using Pazaryeri.Models;
using Pazaryeri.ViewModels;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IProductRepository:IRepository<Product>
    {
        Task<(List<Product> Products, int TotalCount)> GetPagedProductsAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Product> ProductsExistsAsync(string title, Platform platform);
        Task<Product> GetWithDetailsAsync(int id);
        Task<Product> GetByProductMainIdAsync(string productMainId);
        Task<bool> ProductExistsAsync(string productMainId);
        Task SaveGroup(List<IGrouping<string, ProductContent>> Values);
        Task<Product> CreateProductAsync(ProductViewModel model);
        Task<Product> UpdateProductAsync(ProductViewModel model);
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetAllProductsAsync();
        Task AddProductImageAsync(int productId, string imageUrl);
        Task AddVariantImageAsync(int variantId, string imageUrl);
        Task DeleteProductAsync(int id);
        Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null);
        Task SetMainImageAsync(int imageId);
        Task DeleteImageAsync(int imageId);
        Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId);
        Task<ProductVariant>  GetVariantByTempIdAsync(int tempId);
    }
}
