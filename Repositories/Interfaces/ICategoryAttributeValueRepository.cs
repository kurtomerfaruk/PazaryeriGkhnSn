using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface ICategoryAttributeValueRepository
    {
        Task<CategoryAttributeValue> GetByIdAsync(int id);
        Task<CategoryAttributeValue> GetByAttributeValueId(int? id);
    }
}
