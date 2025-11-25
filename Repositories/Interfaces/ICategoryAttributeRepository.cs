
namespace Pazaryeri.Repositories.Interfaces
{
    public interface ICategoryAttributeRepository:IRepository<Models.CategoryAttribute>
    {
        Task<(List<Models.CategoryAttribute> CategoryAttributes, int TotalCount)> GetPagedCategoryAttributesAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task AddOrUpdateRangeAsync(List<Models.CategoryAttribute> categoryAttributes);
        Task SaveChangesAsync();
        Task<Models.CategoryAttribute> GetByCompositeKeyAsync(int categoryId, string attributeId, string? attributeValueId);
    }
}
