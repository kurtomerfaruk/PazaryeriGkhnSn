using Pazaryeri.Dtos;
using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface ICategoryRepository:IRepository<Category>
    {
        Task<List<int>> GetExistingCategoryIdsAsync();
        Task BulkInsertAsync(List<Category> categories);
        Task<CategoryDto?> GetCategoryWithAttributesAsync(int categoryId);
        Task<Category> GetByCategoryIdAsync(int categoryId);
    }
}
