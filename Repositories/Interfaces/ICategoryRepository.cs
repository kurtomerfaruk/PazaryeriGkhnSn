using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface ICategoryRepository:IRepository<Category>
    {
        Task<(List<Category> Categories, int TotalCount)> GetPagedCategoryAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<List<int>> GetExistingCategoryIdsAsync();
        Task BulkInsertAsync(List<Category> categories);
    }
}
