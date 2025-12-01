using Pazaryeri.Models;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<(List<T> Items, int TotalCount)> GetPagedAsync(int start, int length, string search, string sortColumn, string sortDirection);
    }
}
