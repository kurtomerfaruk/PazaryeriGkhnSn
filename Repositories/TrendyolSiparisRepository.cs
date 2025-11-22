using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class TrendyolSiparisRepository : IRepository<TrendyolSiparis>
    {
        private readonly AppDbContext _db;

        public TrendyolSiparisRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(TrendyolSiparis entity)
        {
            _db.TrendyolSiparisler.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.TrendyolSiparisler.FindAsync(id);
            if (p != null)
            {
                _db.TrendyolSiparisler.Remove(p);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<TrendyolSiparis>> GetAllAsync() => await _db.TrendyolSiparisler.ToListAsync();

        public async Task<TrendyolSiparis?> GetByIdAsync(int id) => await _db.TrendyolSiparisler.FindAsync(id);

        public async Task UpdateAsync(TrendyolSiparis entity)
        {
            _db.TrendyolSiparisler.Update(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<TrendyolSiparis> Query(Expression<Func<TrendyolSiparis, bool>>? filter = null)
        {
            return filter == null ? _db.TrendyolSiparisler : _db.TrendyolSiparisler.Where(filter);
        }
    }
}
