using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Product entity)
        {
            _db.Products.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null)
            {
                _db.Products.Remove(p);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> GetAllAsync() => await _db.Products.ToListAsync();

        public async Task<Product?> GetByIdAsync(int id) => await _db.Products.FindAsync(id);

        public async Task UpdateAsync(Product entity)
        {
            _db.Products.Update(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<Product> Query(Expression<Func<Product, bool>>? filter = null)
        {
            return filter == null ? _db.Products : _db.Products.Where(filter);
        }
    }
}
