using Pazaryeri.Models;
using Pazaryeri.Repositories;

namespace Pazaryeri.Services
{
    public class ProductService
    {
        private readonly IRepository<Product> _repo;

        public ProductService(IRepository<Product> repo)
        {
            _repo = repo;
        }

        public Task<List<Product>> GetAll() => _repo.GetAllAsync();
        public Task<Product?> Get(int id) => _repo.GetByIdAsync(id);
        public Task Add(Product p) => _repo.AddAsync(p);
        public Task Update(Product p) => _repo.UpdateAsync(p);
        public Task Delete(int id) => _repo.DeleteAsync(id);
    }
}
