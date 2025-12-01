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
           
            foreach (var attr in categoryAttributes)
            {
                if (attr.Category != null)
                {
                    attr.CategoryId = attr.Category.Id;
                }

                var existingAttr = await _context.CategoryAttributes
                    .Include(a => a.Values)
                    .FirstOrDefaultAsync(a => a.CategoryAttributeId == attr.CategoryAttributeId && a.CategoryId==attr.CategoryId);

                if (existingAttr == null)
                {
                    _context.CategoryAttributes.Add(attr);
                }
                else
                {
                    existingAttr.Name = attr.Name;
                    existingAttr.AllowCustom = attr.AllowCustom;
                    existingAttr.Required = attr.Required;
                    existingAttr.Varianter = attr.Varianter;
                    existingAttr.Slicer = attr.Slicer;
                    existingAttr.AllowMultipleAttributeValues = attr.AllowMultipleAttributeValues;
                    existingAttr.CategoryId = attr.CategoryId;

                    foreach (var val in attr.Values)
                    {
                        val.CategoryAttributeId = existingAttr.Id;
                        val.CategoryAttribute = existingAttr;

                        var existingVal = existingAttr.Values
                            .FirstOrDefault(v => v.CategoryAttributeValueId == val.CategoryAttributeValueId);

                        if (existingVal == null)
                        {
                            existingAttr.Values.Add(val);
                        }
                        else
                        {
                            existingVal.Name = val.Name;
                            existingVal.CategoryAttributeId = existingAttr.Id;
                            existingVal.CategoryAttribute = existingAttr;
                            existingVal.CategoryAttributeValueId=val.CategoryAttributeValueId;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<Models.CategoryAttribute> GetByAttributeIdByCategoryId(int attributeId, int categoryId)
        {
            return await _context.CategoryAttributes.FirstOrDefaultAsync(c=>c.CategoryAttributeId == attributeId && c.CategoryId == categoryId);
        }

    }
}
