using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class CategoryAttributeRepository : ICategoryAttributeRepository
    {
        private readonly AppDbContext _context;

        public CategoryAttributeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryAttribute> CreateAsync(CategoryAttribute entity)
        {
            _context.CategoryAttributes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var categoryAttribute = await GetByIdAsync(id);
            if (categoryAttribute != null)
            {
                _context.CategoryAttributes.Remove(categoryAttribute);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CategoryAttribute>> GetAllAsync()
        {
            return await _context.CategoryAttributes.ToListAsync();
        }

        public async Task<CategoryAttribute> GetByIdAsync(int id)
        {
            return await _context.CategoryAttributes.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<CategoryAttribute> CategoryAttributes, int TotalCount)> GetPagedCategoryAttributesAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.CategoryAttributes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.CategoryId.Equals(search) ||
                   
                    o.Platform.ToString().Contains(search));
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(GetSortProperty(sortColumn))
                    : query.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                query = query.OrderByDescending(o => o.Id);
            }

            var categoryAttributes = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (categoryAttributes, totalCount);
        }

        private static Expression<Func<Models.CategoryAttribute, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "id" => categoryAttribute => categoryAttribute.Id,
                "categoryId" => categoryAttribute => categoryAttribute.CategoryId,
                "platform" => categoryAttribute => categoryAttribute.Platform,
                _ => categoryAttribute => categoryAttribute.Id
            };
        }

        public async Task<CategoryAttribute> UpdateAsync(CategoryAttribute entity)
        {
            _context.CategoryAttributes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task AddOrUpdateRangeAsync(List<CategoryAttribute> categoryAttributes)
        {
            foreach (var attribute in categoryAttributes)
            {
                var existing = await GetByCompositeKeyAsync( attribute.CategoryId);

                if (existing != null)
                {
                    _context.CategoryAttributes.Update(existing);
                }
                else
                {
                    await _context.CategoryAttributes.AddAsync(attribute);
                }
            }
        }

        public async Task<CategoryAttribute> GetByCompositeKeyAsync(long categoryId)
        {
            return await _context.CategoryAttributes
                .FirstOrDefaultAsync(ca =>
                    ca.CategoryId == categoryId &&
                    ca.Platform == Platform.Trendyol);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        
    }
}
