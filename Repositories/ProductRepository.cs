using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.ViewModels;
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
                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(List<Product> Products, int TotalCount)> GetPagedProductsAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Title.Contains(search) ||
                    o.Description.Contains(search));
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
                "description" => product => product.Description,
                _ => product => product.Id
            };
        }

        public async Task<Product> GetWithDetailsAsync(int id)
        {
            return await _context.Products
            .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Product> ProductsExistsAsync(string title, Platform platform)
        {
            return await _context.Products
                 .FirstOrDefaultAsync(o => o.Title == title);
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
                .FirstOrDefaultAsync(p => p.Title == productMainId);
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
                .AnyAsync(p => p.Title == productMainId);
        }

        public async Task SaveGroup(List<IGrouping<string, TrendyolProductDetail>> Values)
        {
            foreach (var group in Values)
            {
                var existingProduct = await _context.Products
                                    .FirstOrDefaultAsync(p => p.Title == group.Key);
                if (existingProduct == null)
                {
                    existingProduct = new Product
                    {
                        Title = group.Key
                    };
                    await CreateAsync(existingProduct);
                }

                var firstGroup = group.First();
                existingProduct.Title = firstGroup.Title;
                existingProduct.Description = firstGroup.Description;

                //foreach (var item in group)
                //{
                //    var existingDetail = existingProduct.TrendyolDetails
                //                .FirstOrDefault(d => d.TrenyolProductId == item.TrenyolProductId);

                //    if (existingDetail == null)
                //    {
                //        existingDetail = item;
                //        existingDetail.Brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == item.BrandId);
                //        existingDetail.Category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == item.CategoryId);
                //        existingProduct.TrendyolDetails.Add(existingDetail);
                //    }
                //    else
                //    {

                //        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == item.BrandId);
                //        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == item.CategoryId);
                //        existingDetail.Barcode = item.Barcode;
                //        existingDetail.BrandId = brand.Id;
                //        existingDetail.CategoryId = category.Id;
                //        existingDetail.Quantity = item.Quantity;
                //        existingDetail.StockCode = item.StockCode;
                //        existingDetail.DimensionalWeight = item.DimensionalWeight;
                //        existingDetail.CurrencyType = item.CurrencyType;
                //        existingDetail.ListPrice = item.ListPrice;
                //        existingDetail.SalePrice = item.SalePrice;
                //        existingDetail.VatRate = item.VatRate;
                //        existingDetail.CargoCompanyId = item.CargoCompanyId;
                //        existingDetail.ShipmentAddressId = item.ShipmentAddressId;
                //        existingDetail.ReturningAddressId = item.ReturningAddressId;
                //        existingDetail.ProductCode = item.ProductCode;
                //        existingDetail.ProductUrl = item.ProductUrl;
                //        existingDetail.SaleStatus = item.SaleStatus;
                //        existingDetail.ApprovalStatus = item.ApprovalStatus;

                //    }


                //}
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Product> CreateProductAsync(ProductViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Ana ürünü oluştur
                var product = new Product
                {
                    Title = model.Title,
                    ProductMainId = model.ProductMainId,
                    ProductCode = model.ProductCode,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    BrandId = model.BrandId.Value,
                    CategoryId = model.CategoryId.Value,
                    CreatedDate = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Özellikleri kaydet
                foreach (var attribute in model.AttributeValues)
                {
                    var productAttribute = new ProductAttribute
                    {
                        ProductId = product.Id,
                        AttributeId = attribute.Key,
                        Value = attribute.Value
                    };
                    _context.ProductAttributes.Add(productAttribute);
                }

                // Varyasyonları kaydet
                foreach (var variantModel in model.Variants)
                {
                    var variant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Sku = variantModel.Sku,
                        Price = variantModel.Price,
                        StockQuantity = variantModel.StockQuantity,
                        Barcode = variantModel.Barcode
                    };

                    _context.ProductVariants.Add(variant);
                    await _context.SaveChangesAsync();

                    // Varyasyon özelliklerini kaydet
                    foreach (var varAttr in variantModel.VariationAttributes)
                    {
                        var variantAttribute = new ProductVariantAttribute
                        {
                            VariantId = variant.Id,
                            AttributeId = varAttr.Key,
                            AttributeValueId = varAttr.Value
                        };
                        _context.ProductVariantAttributes.Add(variantAttribute);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return product;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //public async Task<Product> CreateProductAsync(ProductViewModel model)
        //{
        //    if (await ProductCodeExistsAsync(model.ProductCode))
        //    {
        //        throw new InvalidOperationException("Bu ürün kodu zaten kullanılıyor.");
        //    }
        //    var product = new Product
        //    {
        //        ProductCode = model.ProductCode,
        //        Title = model.Title,
        //        Description = model.Description,
        //        Price = model.Price,
        //        StockQuantity = model.StockQuantity,
        //        BrandId = model.BrandId.Value,
        //        CategoryId = model.CategoryId.Value,
        //        CreatedDate = DateTime.Now
        //    };


        //    if (model.Variants?.Any() == true)
        //    {
        //        foreach (var variantModel in model.Variants)
        //        {
        //            var variant = new ProductVariant
        //            {
        //                Sku = variantModel.Sku,
        //                Price = variantModel.Price,
        //                StockQuantity = variantModel.StockQuantity,
        //                Barcode = variantModel.Barcode
        //            };
        //            product.Variants.Add(variant);
        //        }
        //    }

        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();

        //    return product;
        //}

        public async Task<Product> UpdateProductAsync(ProductViewModel model)
        {
            var product = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (product == null)
            {
                throw new ArgumentException("Ürün bulunamadı.");
            }

            if (await ProductCodeExistsAsync(model.ProductCode, model.Id))
            {
                throw new InvalidOperationException("Bu ürün kodu zaten kullanılıyor.");
            }

            product.ProductCode = model.ProductCode;
            product.Title = model.Title;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.BrandId = model.BrandId.Value;
            product.CategoryId = model.CategoryId.Value;

            await UpdateProductVariantsAsync(product, model.Variants);

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return product;
        }

        private async Task UpdateProductVariantsAsync(Product product, List<ProductVariantViewModel> variantModels)
        {
            foreach (var variantModel in variantModels)
            {
                var existingVariant = product.Variants.FirstOrDefault(v => v.Id == variantModel.Id);

                if (existingVariant != null)
                {
                    existingVariant.Sku = variantModel.Sku;
                    existingVariant.Price = variantModel.Price;
                    existingVariant.StockQuantity = variantModel.StockQuantity;
                    existingVariant.Barcode = variantModel.Barcode;
                }
                else
                {
                    var newVariant = new ProductVariant
                    {
                        Sku = variantModel.Sku,
                        Price = variantModel.Price,
                        StockQuantity = variantModel.StockQuantity,
                        Barcode = variantModel.Barcode,
                        ProductId = product.Id
                    };
                    product.Variants.Add(newVariant);
                }
            }

            var variantIdsToKeep = variantModels.Where(v => v.Id > 0).Select(v => v.Id).ToList();
            var variantsToRemove = product.Variants.Where(v => v.Id > 0 && !variantIdsToKeep.Contains(v.Id)).ToList();

            foreach (var variant in variantsToRemove)
            {
                product.Variants.Remove(variant);
                _context.ProductVariants.Remove(variant);
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.VariantImages)
            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
           .Include(p => p.Images)
           .Include(p => p.Variants)
           .OrderByDescending(p => p.CreatedDate)
           .ToListAsync();
        }

        public async Task AddProductImageAsync(int productId, string imageUrl)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new ArgumentException("Ürün bulunamadı.");
            }

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsMainImage = !product.Images.Any(), 
                SortOrder = product.Images.Count + 1
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
        }

        public async Task AddVariantImageAsync(int variantId, string imageUrl)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null)
            {
                throw new ArgumentException("Varyasyon bulunamadı.");
            }

            var variantImage = new ProductVariantImage
            {
                ProductVariantId = variantId,
                ImageUrl = imageUrl
            };

            _context.ProductVariantImages.Add(variantImage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products
           .Include(p => p.Images)
           .Include(p => p.Variants)
               .ThenInclude(v => v.VariantImages)
           .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new ArgumentException("Ürün bulunamadı.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ProductCodeExistsAsync(string productCode, int? excludeId = null)
        {
            return await _context.Products
            .AnyAsync(p => p.ProductCode == productCode && (!excludeId.HasValue || p.Id != excludeId.Value));
        }
    }
}
