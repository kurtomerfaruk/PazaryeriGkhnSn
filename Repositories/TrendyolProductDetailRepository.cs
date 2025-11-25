using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;

namespace Pazaryeri.Repositories
{
    public class TrendyolProductDetailRepository : ITrendyolProductDetailRepository
    {

        private readonly AppDbContext _context;

        public TrendyolProductDetailRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TrendyolProductDetail> CreateAsync(TrendyolProductDetail entity)
        {
            _context.TrendyolProductDetails.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TrendyolProductDetail> GetByTrendyolIdAsync(string productId)
        {
            return await _context.TrendyolProductDetails
                .Include(o => o.Attributes)
                .Include(o => o.Images)
                .Include(o => o.RejectReasonDetails)
                .FirstOrDefaultAsync(p => p.TrenyolProductId == productId);
        }

        public async Task<TrendyolProductDetail> UpdateAsync(TrendyolProductDetail entity)
        {
            _context.TrendyolProductDetails.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
