using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Pazaryeri.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                 .Include(o => o.TrendyolDetails)
                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<Product> Products, int TotalCount)> GetPagedProductsAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Title.Contains(search) ||
                    o.Subtitle.Contains(search) ||
                    o.Description.Contains(search) ||
                    o.ProductMainId.Contains(search) ||
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

            var products = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (products, totalCount);
        }

        private static Expression<Func<Models.Product, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "id" => product => product.Id,
                "title" => product => product.Title,
                "subtitle" => product => product.Subtitle,
                "description" => product => product.Description,
                "productmainid" => product => product.ProductMainId,
                "platform" => product => product.Platform,
                _ => product => product.Id
            };
        }

        public async Task<Product> GetWithDetailsAsync(int id)
        {
            return await _context.Products
            .Include(o => o.TrendyolDetails).ThenInclude(d => d.Brand)
            .Include(o => o.TrendyolDetails).ThenInclude(d => d.Category)
            .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Product> ProductsExistsAsync(string title, Platform platform)
        {
            return await _context.Products
                 .FirstOrDefaultAsync(o => o.Title == title && o.Platform == platform);
        }

        public async Task<Product> UpdateAsync(Product entity)
        {
            _context.Products.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Product> GetByProductMainIdAsync(string productMainId)
        {
            return await _context.Products
                .Include(o => o.TrendyolDetails).ThenInclude(d => d.Attributes)
                .Include(o => o.TrendyolDetails).ThenInclude(d => d.Images)
                .Include(o => o.TrendyolDetails).ThenInclude(d => d.RejectReasonDetails)
                .FirstOrDefaultAsync(p => p.ProductMainId == productMainId);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> ProductExistsAsync(string productMainId)
        {
            return await _context.Products
                .AnyAsync(p => p.ProductMainId == productMainId);
        }

        public async Task SaveGroup(List<IGrouping<string, TrendyolProductDetail>> Values)
        {
            foreach (var group in Values)
            {
                var existingProduct = await _context.Products
                                    .Include(p => p.TrendyolDetails)
                                    .FirstOrDefaultAsync(p => p.ProductMainId == group.Key);
                if (existingProduct == null)
                {
                    existingProduct = new Product
                    {
                        ProductMainId = group.Key
                    };
                    await CreateAsync(existingProduct);
                }

                var firstGroup = group.First();
                existingProduct.Title = firstGroup.Title;
                existingProduct.Subtitle = firstGroup.Subtitle;
                existingProduct.Description = firstGroup.Description;

                foreach (var item in group)
                {
                    var existingDetail = existingProduct.TrendyolDetails
                                .FirstOrDefault(d => d.TrenyolProductId == item.TrenyolProductId);

                    if (existingDetail == null)
                    {
                        existingDetail = item;
                        existingDetail.Brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == item.BrandId);
                        existingDetail.Category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == item.CategoryId);
                        existingProduct.TrendyolDetails.Add(existingDetail);
                    }
                    else
                    {
                        existingDetail.Brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == item.BrandId);
                        existingDetail.Category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == item.CategoryId);
                        existingDetail.Barcode = item.Barcode;
                        existingDetail.BrandId = item.BrandId;
                        existingDetail.CategoryId = item.CategoryId;
                        existingDetail.Quantity = item.Quantity;
                        existingDetail.StockCode = item.StockCode;
                        existingDetail.DimensionalWeight = item.DimensionalWeight;
                        existingDetail.CurrencyType = item.CurrencyType;
                        existingDetail.ListPrice = item.ListPrice;
                        existingDetail.SalePrice = item.SalePrice;
                        existingDetail.VatRate = item.VatRate;
                        existingDetail.CargoCompanyId = item.CargoCompanyId;
                        existingDetail.ShipmentAddressId = item.ShipmentAddressId;
                        existingDetail.ReturningAddressId = item.ReturningAddressId;
                        existingDetail.ProductCode = item.ProductCode;
                        existingDetail.ProductUrl = item.ProductUrl;
                        existingDetail.SaleStatus = item.SaleStatus;
                        existingDetail.ApprovalStatus = item.ApprovalStatus;
                    }


                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
