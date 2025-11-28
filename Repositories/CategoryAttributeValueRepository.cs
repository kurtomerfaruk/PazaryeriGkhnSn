using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;

namespace Pazaryeri.Repositories
{
    public class CategoryAttributeValueRepository : ICategoryAttributeValueRepository
    {
        private readonly AppDbContext _context;

        public CategoryAttributeValueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryAttributeValue> GetByIdAsync(int id)
        {
            return await _context.CategoryAttributeValue
                .Include(c=>c.CategoryAttribute)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
