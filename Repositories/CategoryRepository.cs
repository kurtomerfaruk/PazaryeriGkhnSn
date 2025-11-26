using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Dtos;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {

        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Category> CreateAsync(Category entity)
        {
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                  .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<Category> Categories, int TotalCount)> GetPagedCategoryAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.CategoryCode.Contains(search) ||
                    o.Title.Contains(search) ||
                    o.ParentCategoryId.Contains(search) ||
                    o.CategoryId.Equals(search) ||
                    o.TopCategory.Equals(search) ||
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

            var categories = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (categories, totalCount);
        }

        private static Expression<Func<Category, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "id" => category => category.Id,
                "categoryid" => category => category.CategoryId,
                "categorycode" => category => category.CategoryCode,
                "title" => category => category.Title,
                "parentcategoryid" => category => category.ParentCategoryId,
                "topcategory" => category => category.TopCategory,
                "platform" => category => category.Platform,
                _ => category => category.Id
            };
        }

        public async Task<Category> UpdateAsync(Category entity)
        {
            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<int>> GetExistingCategoryIdsAsync()
        {
            return await _context.Categories
                    .Select(c => c.CategoryId)
                    .ToListAsync();
        }

        public async Task BulkInsertAsync(List<Category> categories)
        {
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
        }

        public async Task<CategoryDto> GetCategoryWithAttributesAsync(int categoryId)
        {
            var category = await _context.Categories
                 .Include(c => c.CategoryAttributes)
                 .ThenInclude(a => a.Values)
                 .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null) return null;

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Title,
                CategoryAttributes = category.CategoryAttributes.Select(a => new CategoryAttributeDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    AllowCustom = a.AllowCustom,
                    Required = a.Required,
                    Platform = a.Platform,
                    AllowMultipleAttributeValues = a.AllowMultipleAttributeValues,
                    Varianter = a.Varianter,
                    Values = a.Values.Select(v => new CategoryAttributeValueDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                    }).ToList()
                }).ToList()
            };

            return dto;
        }
    }
}
