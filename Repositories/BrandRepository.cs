using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class BrandRepository : IBrandRepository
    {

        private readonly AppDbContext _context;

        public BrandRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Brand> CreateAsync(Brand entity)
        {
            _context.Brands.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var brand = await GetByIdAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _context.Brands.ToListAsync();
        }

        public async Task<Brand> GetByIdAsync(int id)
        {
            return await _context.Brands
                 .FirstOrDefaultAsync(o => o.Id == id);
        }

       

        public async Task<Brand> UpdateAsync(Brand entity)
        {
            _context.Brands.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<(List<Brand> Brands, int TotalCount)> GetPagedBrandsAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Brands.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Name.Contains(search) ||
                    o.BrandId.Equals(search) ||
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

            var brands = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (brands, totalCount);
        }

        private static Expression<Func<Models.Brand, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "id" => brand => brand.Id,
                "brandid" => brand => brand.BrandId,
                "name" => brand => brand.Name,
                "platform" => brand => brand.Platform,
                _ => brand => brand.Id
            };
        }

        public async Task<Brand> BrandExistsAsync(string brandName)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(o => o.Name == brandName);
        }
    }
}
