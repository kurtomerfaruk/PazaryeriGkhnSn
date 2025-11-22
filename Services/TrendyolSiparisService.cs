using Pazaryeri.Models;
using Pazaryeri.Repositories;

namespace Pazaryeri.Services
{
    public class TrendyolSiparisService
    {
        private readonly IRepository<TrendyolSiparis> _repo;

        public TrendyolSiparisService(IRepository<TrendyolSiparis> repo)
        {
            _repo = repo;
        }

        public Task<List<TrendyolSiparis>> GetAll() => _repo.GetAllAsync();
        public Task<TrendyolSiparis?> Get(int id) => _repo.GetByIdAsync(id);
        public Task Add(TrendyolSiparis p) => _repo.AddAsync(p);
        public Task Update(TrendyolSiparis p) => _repo.UpdateAsync(p);
        public Task Delete(int id) => _repo.DeleteAsync(id);
    }
}
