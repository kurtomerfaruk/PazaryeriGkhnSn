
namespace Pazaryeri.Repositories.Interfaces
{
    public interface ICategoryAttributeRepository:IRepository<Models.CategoryAttribute>
    {
        Task AddOrUpdateRangeAsync(List<Models.CategoryAttribute> categoryAttributes);
        Task SaveChangesAsync();
        Task<Models.CategoryAttribute> GetByAttributeIdByCategoryId(int attributeId,int categoryId);
    }
}
