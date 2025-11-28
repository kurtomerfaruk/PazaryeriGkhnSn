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
            return await _context.Products.FirstOrDefaultAsync(o => o.Id == id);
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

        //public async Task SaveGroup(List<IGrouping<string, TrendyolProductDetail>> Values)
        //{
        //    foreach (var group in Values)
        //    {
        //        var existingProduct = await _context.Products
        //                            .FirstOrDefaultAsync(p => p.Title == group.Key);
        //        if (existingProduct == null)
        //        {
        //            existingProduct = new Product
        //            {
        //                Title = group.Key
        //            };
        //            await CreateAsync(existingProduct);
        //        }

        //        var firstGroup = group.First();
        //        existingProduct.Title = firstGroup.Title;
        //        existingProduct.Description = firstGroup.Description;

        //        //foreach (var item in group)
        //        //{
        //        //    var existingDetail = existingProduct.TrendyolDetails
        //        //                .FirstOrDefault(d => d.TrenyolProductId == item.TrenyolProductId);

        //        //    if (existingDetail == null)
        //        //    {
        //        //        existingDetail = item;
        //        //        existingDetail.Brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == item.BrandId);
        //        //        existingDetail.Category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == item.CategoryId);
        //        //        existingProduct.TrendyolDetails.Add(existingDetail);
        //        //    }
        //        //    else
        //        //    {

        //        //        var brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == item.BrandId);
        //        //        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == item.CategoryId);
        //        //        existingDetail.Barcode = item.Barcode;
        //        //        existingDetail.BrandId = brand.Id;
        //        //        existingDetail.CategoryId = category.Id;
        //        //        existingDetail.Quantity = item.Quantity;
        //        //        existingDetail.StockCode = item.StockCode;
        //        //        existingDetail.DimensionalWeight = item.DimensionalWeight;
        //        //        existingDetail.CurrencyType = item.CurrencyType;
        //        //        existingDetail.ListPrice = item.ListPrice;
        //        //        existingDetail.SalePrice = item.SalePrice;
        //        //        existingDetail.VatRate = item.VatRate;
        //        //        existingDetail.CargoCompanyId = item.CargoCompanyId;
        //        //        existingDetail.ShipmentAddressId = item.ShipmentAddressId;
        //        //        existingDetail.ReturningAddressId = item.ReturningAddressId;
        //        //        existingDetail.ProductCode = item.ProductCode;
        //        //        existingDetail.ProductUrl = item.ProductUrl;
        //        //        existingDetail.SaleStatus = item.SaleStatus;
        //        //        existingDetail.ApprovalStatus = item.ApprovalStatus;

        //        //    }


        //        //}
        //    }
        //    await _context.SaveChangesAsync();
        //}


        public async Task<Product> CreateProductAsync(ProductViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. ANA ÜRÜNÜ OLUŞTUR
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
                    TrendyolCargoId = model.TrendyolCargoId.Value,
                    CreatedDate = DateTime.Now,
                    Active = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // ProductId'yi almak için

                // 2. ÜRÜN ATTRIBUTE'LARINI KAYDET
                if (model.AttributeValues != null && model.AttributeValues.Any())
                {
                    foreach (var attribute in model.AttributeValues)
                    {
                        var productAttribute = new ProductAttribute
                        {
                            ProductId = product.Id,
                            AttributeId = attribute.Key,
                            Value = attribute.Value,
                            CreatedDate = DateTime.Now
                        };
                        _context.ProductAttributes.Add(productAttribute);
                    }
                    await _context.SaveChangesAsync();
                }

                // 3. VARYASYONLARI KAYDET
                if (model.Variants != null && model.Variants.Any())
                {
                    foreach (var variantModel in model.Variants)
                    {
                        var variant = new ProductVariant
                        {
                            ProductId = product.Id,
                            Sku = variantModel.Sku,
                            ListPrice = variantModel.ListPrice,
                            SalePrice = variantModel.SalePrice,
                            StockQuantity = variantModel.StockQuantity,
                            Barcode = variantModel.Barcode,
                            CreatedDate = DateTime.Now,
                            Active = true
                        };

                        _context.ProductVariants.Add(variant);
                        await _context.SaveChangesAsync(); // VariantId'yi almak için

                        // 4. VARYASYON ATTRIBUTE'LARINI KAYDET
                        if (variantModel.VariationAttributes != null && variantModel.VariationAttributes.Any())
                        {
                            foreach (var varAttr in variantModel.VariationAttributes)
                            {
                                var variantAttribute = new ProductVariantAttribute
                                {
                                    VariantId = variant.Id,
                                    AttributeId = varAttr.Key,
                                    AttributeValueId = varAttr.Value,
                                    CreatedDate = DateTime.Now
                                };
                                _context.ProductVariantAttributes.Add(variantAttribute);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // 5. RESİMLERİ KAYDET (Temp URL'lerden)
                await SaveProductImages(product.Id, model.TempProductImageUrls);
                await SaveVariantImages(model);

                // TÜM İŞLEMLER BAŞARILI - COMMIT
                await transaction.CommitAsync();

                return product;
            }
            catch (Exception ex)
            {
                // HATA DURUMUNDA - ROLLBACK
                await transaction.RollbackAsync();
                throw new Exception($"Ürün kaydı sırasında hata oluştu: {ex.Message}", ex);
            }
        }

        private async Task SaveProductImages(int productId, List<string> tempImageUrls)
        {
            if (tempImageUrls != null && tempImageUrls.Any())
            {
                foreach (var imageUrl in tempImageUrls)
                {
                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = imageUrl,
                        IsMainImage = false, // İlk resmi main yapabilirsiniz
                        SortOrder = 0,
                        CreatedDate = DateTime.Now
                    };
                    _context.ProductImages.Add(productImage);
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task SaveVariantImages(ProductViewModel model)
        {
            if (model.TempVariantImageUrls != null && model.TempVariantImageUrls.Any())
            {
                // Tüm varyasyonları productId ile birlikte al
                var variants = _context.ProductVariants
                    .Where(v => v.ProductId == model.Id)
                    .ToList();

                foreach (var variantImage in model.TempVariantImageUrls)
                {
                    var variant = variants.FirstOrDefault(v => v.TempId == variantImage.Key);
                    if (variant != null)
                    {
                        foreach (var imageUrl in variantImage.Value)
                        {
                            var variantImageEntity = new ProductVariantImage
                            {
                                ProductVariantId = variant.Id,
                                ImageUrl = imageUrl,
                                CreatedDate = DateTime.Now
                            };
                            _context.ProductVariantImages.Add(variantImageEntity);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
        }


        public async Task<Product> UpdateProductAsync(ProductViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. MEVCUT ÜRÜNÜ BUL
                var product = await _context.Products
                    .Include(p => p.Attributes)
                    .Include(p => p.Variants)
                        .ThenInclude(v => v.VariantAttributes)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (product == null)
                    throw new Exception("Ürün bulunamadı!");

                // 2. ANA ÜRÜN BİLGİLERİNİ GÜNCELLE
                product.Title = model.Title;
                product.ProductMainId = model.ProductMainId;
                product.ProductCode = model.ProductCode;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.BrandId = model.BrandId.Value;
                product.CategoryId = model.CategoryId.Value;
                product.TrendyolCargoId = model.TrendyolCargoId.Value;
                product.UpdatedDate = DateTime.Now;

                // 3. ÜRÜN ATTRIBUTE'LARINI GÜNCELLE (SİL ve YENİDEN EKLE)
                var existingAttributes = _context.ProductAttributes
                    .Where(pa => pa.ProductId == product.Id);
                _context.ProductAttributes.RemoveRange(existingAttributes);

                if (model.AttributeValues != null && model.AttributeValues.Any())
                {
                    foreach (var attribute in model.AttributeValues)
                    {
                        var productAttribute = new ProductAttribute
                        {
                            ProductId = product.Id,
                            AttributeId = attribute.Key,
                            Value = attribute.Value,
                            CreatedDate = DateTime.Now
                        };
                        _context.ProductAttributes.Add(productAttribute);
                    }
                }

                // 4. VARYASYON İŞLEMLERİ
                await UpdateProductVariants(product.Id, model);

                // 5. YENİ RESİMLERİ EKLE
                if (model.TempProductImageUrls != null && model.TempProductImageUrls.Any())
                {
                    await SaveProductImages(product.Id, model.TempProductImageUrls);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return product;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Ürün güncelleme sırasında hata oluştu: {ex.Message}", ex);
            }
        }

        private async Task UpdateProductVariants(int productId, ProductViewModel model)
        {
            // Mevcut varyasyonları al
            var existingVariants = await _context.ProductVariants
                .Include(v => v.VariantAttributes)
                .Include(v => v.VariantImages)
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            // Güncellenecek varyasyon ID'leri
            var updatedVariantIds = model.Variants?.Select(v => v.TempId).ToList() ?? new List<int>();

            // Silinecek varyasyonları bul ve sil
            var variantsToDelete = existingVariants.Where(ev => !updatedVariantIds.Contains(ev.TempId)).ToList();
            foreach (var variantToDelete in variantsToDelete)
            {
                _context.ProductVariantAttributes.RemoveRange(variantToDelete.VariantAttributes);
                _context.ProductVariantImages.RemoveRange(variantToDelete.VariantImages);
                _context.ProductVariants.Remove(variantToDelete);
            }

            if (model.Variants != null)
            {
                foreach (var variantModel in model.Variants)
                {
                    var existingVariant = existingVariants.FirstOrDefault(ev => ev.TempId == variantModel.TempId);

                    if (existingVariant != null)
                    {
                        existingVariant.Sku = variantModel.Sku;
                        existingVariant.ListPrice = variantModel.ListPrice;
                        existingVariant.SalePrice = variantModel.SalePrice;
                        existingVariant.StockQuantity = variantModel.StockQuantity;
                        existingVariant.Barcode = variantModel.Barcode;
                        existingVariant.UpdatedDate = DateTime.Now;

                        // Varyasyon attribute'larını güncelle
                        _context.ProductVariantAttributes.RemoveRange(existingVariant.VariantAttributes);

                        if (variantModel.VariationAttributes != null)
                        {
                            foreach (var varAttr in variantModel.VariationAttributes)
                            {
                                var variantAttribute = new ProductVariantAttribute
                                {
                                    VariantId = existingVariant.Id,
                                    AttributeId = varAttr.Key,
                                    AttributeValueId = varAttr.Value,
                                    CreatedDate = DateTime.Now
                                };
                                _context.ProductVariantAttributes.Add(variantAttribute);
                            }
                        }
                    }
                    else
                    {
                        // YENİ VARYASYON EKLE
                        var newVariant = new ProductVariant
                        {
                            ProductId = productId,
                            Sku = variantModel.Sku,
                            ListPrice = variantModel.ListPrice,
                            SalePrice = variantModel.SalePrice,
                            StockQuantity = variantModel.StockQuantity,
                            Barcode = variantModel.Barcode,
                            TempId = variantModel.TempId,
                            CreatedDate = DateTime.Now,
                            Active = true
                        };
                        _context.ProductVariants.Add(newVariant);
                        await _context.SaveChangesAsync();

                        // Varyasyon attribute'larını ekle
                        if (variantModel.VariationAttributes != null)
                        {
                            foreach (var varAttr in variantModel.VariationAttributes)
                            {
                                var variantAttribute = new ProductVariantAttribute
                                {
                                    VariantId = newVariant.Id,
                                    AttributeId = varAttr.Key,
                                    AttributeValueId = varAttr.Value,
                                    CreatedDate = DateTime.Now
                                };
                                _context.ProductVariantAttributes.Add(variantAttribute);
                            }
                        }

                        // Varyasyon resimlerini ekle
                        if (model.TempVariantImageUrls != null &&
                            model.TempVariantImageUrls.ContainsKey(variantModel.TempId))
                        {
                            foreach (var imageUrl in model.TempVariantImageUrls[variantModel.TempId])
                            {
                                var variantImage = new ProductVariantImage
                                {
                                    ProductVariantId = newVariant.Id,
                                    ImageUrl = imageUrl,
                                    CreatedDate = DateTime.Now
                                };
                                _context.ProductVariantImages.Add(variantImage);
                            }
                        }
                    }
                }
            }
        }



        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants).ThenInclude(v => v.VariantImages)
                .Include(p => p.Variants).ThenInclude(c => c.VariantAttributes)
                .Include(p => p.Attributes)
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

        public async Task SetMainImageAsync(int imageId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Önce tüm resimlerin main özelliğini kaldır
                var productImages = await _context.ProductImages
                    .Where(pi => pi.ProductId == _context.ProductImages
                        .Where(pi2 => pi2.Id == imageId)
                        .Select(pi2 => pi2.ProductId)
                        .FirstOrDefault())
                    .ToListAsync();

                foreach (var image in productImages)
                {
                    image.IsMainImage = false;
                }

                // Seçilen resmi main yap
                var mainImage = await _context.ProductImages.FindAsync(imageId);
                if (mainImage != null)
                {
                    mainImage.IsMainImage = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteImageAsync(int imageId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var image = await _context.ProductImages.FindAsync(imageId);
                if (image != null)
                {
                    _context.ProductImages.Remove(image);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();
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
                ImageUrl = imageUrl,
                CreatedDate = DateTime.Now
            };

            _context.ProductVariantImages.Add(variantImage);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductVariant> GetVariantByTempIdAsync(int tempId)
        {
            return await _context.ProductVariants.Include(c => c.VariantImages).FirstOrDefaultAsync(p => p.TempId == tempId); ;
        }
    }
}
