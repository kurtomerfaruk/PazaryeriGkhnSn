using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pazaryeri.Dtos;
using Pazaryeri.Entity.Trendyol.Request;
using Pazaryeri.Entity.Trendyol.Response;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.Requests;
using Pazaryeri.Services;
using Pazaryeri.ViewModels;
using System.Globalization;

namespace Pazaryeri.Controllers
{
    public class ProductController : BaseController
    {

        private readonly IProductRepository _productRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITrendyolProductDetailRepository _trendyolProductDetailRepository;
        private readonly IPlatformServiceFactory _platformServiceFactory;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(IProductRepository productRepository,
            IImageRepository imageRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<ProductController> logger,
            IConfiguration configuration,
            ITrendyolProductDetailRepository trendyolProductDetailRepository)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _platformServiceFactory = platformServiceFactory;
            _logger = logger;
            _configuration = configuration;
            _trendyolProductDetailRepository = trendyolProductDetailRepository;
        }
        public IActionResult Index()
        {
            SetActiveMenu("Product");
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var (products, totalCount) = await _productRepository.GetPagedProductsAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = products.Select(o => new
                    {
                        id = o.Id,
                        title = o.Title,
                        actions = o.Id
                    })
                };

                return Json(returnObj);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FetchTrendyolProducts()
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                var productGroups = await trendyolService.GetGroupedProductsAsync();
                int addedProducts = 0;
                int updatedProducts = 0;
                int addedDetails = 0;
                int updatedDetails = 0;

                await _productRepository.SaveGroup(productGroups);

                return Json(new
                {
                    success = true,
                    message = $"Trendyol için {addedProducts} yeni ürün eklendi, {updatedProducts} ürün güncellendi. " +
                             $"{addedDetails} yeni detay eklendi, {updatedDetails} detay güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol ürünleri çekilirken hata: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                var product = await _productRepository.GetWithDetailsAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı" });
                }

                var result = new
                {
                    success = true,
                    product = new
                    {
                        id = product.Id,
                        title = product.Title,
                        description = product.Description,
                    },


                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Lütfen bir dosya seçin." });
            }

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Sadece Excel dosyaları (.xlsx, .xls) yükleyebilirsiniz." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Dosya boyutu 5MB'tan büyük olamaz." });
            }


            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet(1);

            var stockPriceList = new List<StockPriceRequest>();

            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var raw = row.Cell(3).GetValue<string>();
                decimal price = decimal.Parse(raw, new CultureInfo("tr-TR"));
                var product = new StockPriceRequest
                {
                    barcode = row.Cell(1).GetValue<string>(),
                    quantity = row.Cell(2).GetValue<int>(),
                    listPrice = row.Cell(3).GetValue<decimal>(),
                    salePrice = row.Cell(4).GetValue<decimal>(),
                };
                stockPriceList.Add(product);
            }

            if (!stockPriceList.Any())
            {
                return BadRequest(new { message = "Excel dosyasında işlenecek veri bulunamadı." });
            }

            var trendyolService = _platformServiceFactory.GetTrendyolService();
            BatchRequestIdResponse result = await trendyolService.UpdateStockPrice(stockPriceList);

            if (result != null)
            {
                StockPriceBatchResponse batchResponse = await trendyolService.UpdateStockPriceBatchResult(result.batchRequestId);
                return Ok(new
                {
                    message = $"{stockPriceList.Count} ürün başarıyla güncellendi.",
                    batchRequestId = batchResponse.batchRequestId,
                    processedItems = stockPriceList.Count,
                    batchItems = batchResponse.items
                });
            }

            return Json(new
            {
                success = true,
                message = $"Trendyol için  detay güncellendi."
            });
        }




        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel
            {
                Variants = new List<ProductVariantViewModel>(),
                CategoryAttributes = new List<CategoryAttributeViewModel>(),
                AttributeValues = new Dictionary<int, string>(),
                VariantIds = new List<int>()
            };
            await LoadBrandsAsync(model);
            await LoadCategoriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            await LoadBrandsAsync(model);
            await LoadCategoriesAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.VariantIds != null && model.VariantIds.Any())
                    {
                        model.Variants = model.VariantIds.Select(id =>
                        {
                            return new ProductVariantViewModel
                            {
                                TempId = id,
                                Sku = HttpContext.Request.Form[$"Variants[{id}].Sku"],
                                Price = decimal.Parse(HttpContext.Request.Form[$"Variants[{id}].Price"]),
                                StockQuantity = int.Parse(HttpContext.Request.Form[$"Variants[{id}].StockQuantity"]),
                                Barcode = HttpContext.Request.Form[$"Variants[{id}].Barcode"],
                                VariationAttributes = ParseVariationAttributes(HttpContext.Request.Form, id)
                            };
                        }).ToList();
                    }

                    var product = await _productRepository.CreateProductAsync(model);

                    if (model.ImageFiles?.Any() == true)
                        await UploadProductImages(product.Id, model.ImageFiles);

                    TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ürün oluşturulurken hata oluştu: {ex.Message}");
                    _logger.LogError(ex, "Ürün oluşturulurken hata oluştu");
                }
            }


            await RestoreVariantsFromRequest(model);

            await RestoreModelData(model);

            return View(model);
        }

        private async Task RestoreModelData(ProductViewModel model)
        {
            // 1. ÖNCE kategori attribute'larını yükle
            if (model.CategoryId.HasValue)
            {
                var categoryDto = await _categoryRepository.GetCategoryWithAttributesAsync(model.CategoryId.Value);
                model.CategoryAttributes = categoryDto.CategoryAttributes.Select(a => new CategoryAttributeViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Varianter = a.Varianter,
                    AllowCustom = a.AllowCustom,
                    Required = a.Required,
                    AllowMultipleAttributeValues = a.AllowMultipleAttributeValues,
                    Values = a.Values.Select(v => new CategoryAttributeValueViewModel
                    {
                        Id = v.Id,
                        Name = v.Name
                    }).ToList()
                }).ToList();
            }

            // 2. Varyasyonları geri yükle
            model.Variants = new List<ProductVariantViewModel>();
            model.VariantIds = new List<int>();

            var variantIdsValues = HttpContext.Request.Form["VariantIds"];
            if (!string.IsNullOrEmpty(variantIdsValues))
            {
                foreach (var variantIdStr in variantIdsValues)
                {
                    if (int.TryParse(variantIdStr, out int variantId))
                    {
                        model.VariantIds.Add(variantId);
                        var variant = new ProductVariantViewModel
                        {
                            TempId = variantId,
                            Sku = HttpContext.Request.Form[$"Variants[{variantId}].Sku"],
                            Price = decimal.TryParse(HttpContext.Request.Form[$"Variants[{variantId}].Price"], out decimal price) ? price : 0,
                            StockQuantity = int.TryParse(HttpContext.Request.Form[$"Variants[{variantId}].StockQuantity"], out int stock) ? stock : 0,
                            Barcode = HttpContext.Request.Form[$"Variants[{variantId}].Barcode"],
                            VariationAttributes = new Dictionary<int, int>()
                        };

                        // Varyasyon attribute'larını geri yükle
                        foreach (var key in HttpContext.Request.Form.Keys)
                        {
                            if (key.StartsWith($"Variants[{variantId}].VariationAttributes["))
                            {
                                var attrMatch = System.Text.RegularExpressions.Regex.Match(key, @"VariationAttributes\[(\d+)\]");
                                if (attrMatch.Success && int.TryParse(attrMatch.Groups[1].Value, out int attrId))
                                {
                                    if (int.TryParse(HttpContext.Request.Form[key], out int attrValueId))
                                    {
                                        variant.VariationAttributes[attrId] = attrValueId;
                                    }
                                }
                            }
                        }
                        model.Variants.Add(variant);
                    }
                }
            }

            // 3. Normal attribute'ları geri yükle
            model.AttributeValues = new Dictionary<int, string>();
            foreach (var key in HttpContext.Request.Form.Keys)
            {
                if (key.StartsWith("AttributeValues["))
                {
                    var attrMatch = System.Text.RegularExpressions.Regex.Match(key, @"AttributeValues\[(\d+)\]");
                    if (attrMatch.Success && int.TryParse(attrMatch.Groups[1].Value, out int attrId))
                    {
                        model.AttributeValues[attrId] = HttpContext.Request.Form[key];
                    }
                }
            }

            // 4. RESİMLERİ GERİ YÜKLE - YENİ EKLENEN KISIM
            await RestoreImages(model);
        }

        private async Task RestoreImages(ProductViewModel model)
        {
            try
            {
                // Ürün resimlerini geri yükle
                if (HttpContext.Request.Form.Files.Count > 0)
                {
                    model.TempProductImageUrls = new List<string>();

                    foreach (var file in HttpContext.Request.Form.Files)
                    {
                        if (file.Name == "ImageFiles" && file.Length > 0)
                        {
                            // Resmi kaydet ve URL'sini al
                            var imageUrl = await _imageRepository.UploadImageAsync(file);
                            model.TempProductImageUrls.Add(imageUrl);
                        }
                    }
                }

                // Varyasyon resimlerini geri yükle
                model.TempVariantImageUrls = new Dictionary<int, List<string>>();

                foreach (var variantId in model.VariantIds)
                {
                    var variantImageUrls = new List<string>();

                    foreach (var file in HttpContext.Request.Form.Files)
                    {
                        if (file.Name.StartsWith($"Variants[{variantId}].ImageFiles") && file.Length > 0)
                        {
                            var imageUrl = await _imageRepository.UploadImageAsync(file);
                            variantImageUrls.Add(imageUrl);
                        }
                    }

                    if (variantImageUrls.Any())
                    {
                        model.TempVariantImageUrls[variantId] = variantImageUrls;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resimler geri yüklenirken hata oluştu");
            }
        }

        private async Task RestoreVariantsFromRequest(ProductViewModel model)
        {
            try
            {
                model.Variants = new List<ProductVariantViewModel>();
                model.VariantIds = new List<int>();

                var variantIdsValues = HttpContext.Request.Form["VariantIds"];
                if (!string.IsNullOrEmpty(variantIdsValues))
                {
                    foreach (var variantIdStr in variantIdsValues)
                    {
                        if (int.TryParse(variantIdStr, out int variantId))
                        {
                            model.VariantIds.Add(variantId);

                            var variant = new ProductVariantViewModel
                            {
                                TempId = variantId,
                                Sku = HttpContext.Request.Form[$"Variants[{variantId}].Sku"],
                                Price = decimal.TryParse(HttpContext.Request.Form[$"Variants[{variantId}].Price"], out decimal price) ? price : 0,
                                StockQuantity = int.TryParse(HttpContext.Request.Form[$"Variants[{variantId}].StockQuantity"], out int stock) ? stock : 0,
                                Barcode = HttpContext.Request.Form[$"Variants[{variantId}].Barcode"],
                                VariationAttributes = new Dictionary<int, int>()
                            };

                            foreach (var key in HttpContext.Request.Form.Keys)
                            {
                                if (key.StartsWith($"Variants[{variantId}].VariationAttributes["))
                                {
                                    var attrMatch = System.Text.RegularExpressions.Regex.Match(key, @"VariationAttributes\[(\d+)\]");
                                    if (attrMatch.Success && int.TryParse(attrMatch.Groups[1].Value, out int attrId))
                                    {
                                        if (int.TryParse(HttpContext.Request.Form[key], out int attrValueId))
                                        {
                                            variant.VariationAttributes[attrId] = attrValueId;
                                        }
                                    }
                                }
                            }

                            model.Variants.Add(variant);
                        }
                    }
                }

                model.AttributeValues = new Dictionary<int, string>();
                foreach (var key in HttpContext.Request.Form.Keys)
                {
                    if (key.StartsWith("AttributeValues["))
                    {
                        var attrMatch = System.Text.RegularExpressions.Regex.Match(key, @"AttributeValues\[(\d+)\]");
                        if (attrMatch.Success && int.TryParse(attrMatch.Groups[1].Value, out int attrId))
                        {
                            model.AttributeValues[attrId] = HttpContext.Request.Form[key];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varyasyon verileri geri yüklenirken hata oluştu");
            }
        }

        private async Task LoadCategoryAttributesAsync(ProductViewModel model)
        {
            try
            {
                var categoryDto = await _categoryRepository.GetCategoryWithAttributesAsync(model.CategoryId.Value);
                model.CategoryAttributes = categoryDto.CategoryAttributes.Select(a => new CategoryAttributeViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Varianter = a.Varianter,
                    AllowCustom = a.AllowCustom,
                    Required = a.Required,
                    AllowMultipleAttributeValues = a.AllowMultipleAttributeValues,
                    Values = a.Values.Select(v => new CategoryAttributeValueViewModel
                    {
                        Id = v.Id,
                        Name = v.Name
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori özellikleri yüklenirken hata oluştu");
                model.CategoryAttributes = new List<CategoryAttributeViewModel>();
            }
        }


        private Dictionary<int, int> ParseVariationAttributes(IFormCollection form, int variantId)
        {
            var attributes = new Dictionary<int, int>();

            foreach (var key in form.Keys)
            {
                if (key.StartsWith($"Variants[{variantId}].VariationAttributes["))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(key, @"VariationAttributes\[(\d+)\]");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int attrId))
                    {
                        if (int.TryParse(form[key], out int valueId))
                        {
                            attributes[attrId] = valueId;
                        }
                    }
                }
            }

            return attributes;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = MapToViewModel(product);

            await LoadBrandsAsync(model);
            await LoadCategoriesAsync(model);

            if (model.CategoryId.HasValue)
            {
                await LoadCategoryAttributesAsync(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            await LoadBrandsAsync(model);
            await LoadCategoriesAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    // Varyasyonları request'ten al
                    if (model.VariantIds != null && model.VariantIds.Any())
                    {
                        model.Variants = model.VariantIds.Select(id =>
                        {
                            return new ProductVariantViewModel
                            {
                                TempId = id,
                                Sku = HttpContext.Request.Form[$"Variants[{id}].Sku"],
                                Price = decimal.Parse(HttpContext.Request.Form[$"Variants[{id}].Price"]),
                                StockQuantity = int.Parse(HttpContext.Request.Form[$"Variants[{id}].StockQuantity"]),
                                Barcode = HttpContext.Request.Form[$"Variants[{id}].Barcode"],
                                VariationAttributes = ParseVariationAttributes(HttpContext.Request.Form, id)
                            };
                        }).ToList();
                    }

                    // Resimleri geri yükle
                    await RestoreImages(model);

                    // Güncelleme işlemi
                    var product = await _productRepository.UpdateProductAsync(model);

                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ürün güncellenirken hata oluştu: {ex.Message}");
                    _logger.LogError(ex, "Ürün güncellenirken hata oluştu");
                }
            }

            // ModelState invalid ise verileri geri yükle
            await RestoreModelData(model);
            return View(model);
        }

        private async Task UploadProductImages(int productId, List<IFormFile> imageFiles)
        {
            foreach (var file in imageFiles)
            {
                if (file.Length > 0)
                {
                    var imageUrl = await _imageRepository.UploadImageAsync(file);
                    await _productRepository.AddProductImageAsync(productId, imageUrl);
                }
            }
        }

        private async Task UploadVariantImages(int variantId, List<IFormFile> imageFiles)
        {
            foreach (var file in imageFiles)
            {
                if (file.Length > 0)
                {
                    var imageUrl = await _imageRepository.UploadImageAsync(file);
                    await _productRepository.AddVariantImageAsync(variantId, imageUrl);
                }
            }
        }

        private ProductViewModel MapToViewModel(Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductMainId=product.ProductMainId,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                BrandId = product.BrandId,
                CategoryId = product.CategoryId,
                ExistingImages = product.Images.Select(i => new ProductImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMainImage = i.IsMainImage,
                    SortOrder = i.SortOrder
                }).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantViewModel
                {
                    Id = v.Id,
                    TempId = v.TempId,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Barcode = v.Barcode,
                    ExistingImages = v.VariantImages.Select(vi => vi.ImageUrl).ToList(),
                    VariationAttributes = v.VariantAttributes.ToDictionary(va => va.AttributeId, va => va.AttributeValueId)
                }).ToList(),
                AttributeValues = product.Attributes.ToDictionary(a=>a.AttributeId,a=>a.Value ?? "")
            };
        }


        [HttpPost]
        public async Task<IActionResult> AddVariant([FromForm] ProductViewModel model)
        {
            if (model == null)
            {
                model = new ProductViewModel();
            }

            await LoadBrandsAsync(model);
            await LoadCategoriesAsync(model);

            model.Variants ??= new List<ProductVariantViewModel>();

            model.Variants.Add(new ProductVariantViewModel());

            return PartialView("_VariantPartial", model);
        }

        private async Task LoadBrandsAsync(ProductViewModel model)
        {
            var brands = await _brandRepository.GetAllAsync();
            model.Brands = brands.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();

            model.Brands.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Marka Seçiniz...",
                Selected = model.BrandId == null
            });
        }

        private async Task LoadCategoriesAsync(ProductViewModel model)
        {
            var categories = await _categoryRepository.GetAllAsync();
            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title
            }).ToList();

            model.Categories.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Kategori Seçiniz...",
                Selected = model.CategoryId == null
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryAttributes(int categoryId)
        {
            CategoryDto categoryDto = await _categoryRepository.GetCategoryWithAttributesAsync(categoryId);

            if (categoryDto == null) return NotFound();

            return Json(new
            {
                success = true,
                data = categoryDto
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetVariantEditor([FromBody] VariantEditorRequest request)
        {
            try
            {
                request ??= new VariantEditorRequest();

                var variant = request.VariantData ?? new ProductVariantViewModel();
                variant.TempId = request.TempId > 0 ? request.TempId : 1;

                var variationAttributes = request.VariationAttributes ?? new List<CategoryAttributeViewModel>();

                ViewBag.VariationAttributes = variationAttributes;
                return PartialView("_VariantEditor", variant);
            }
            catch (Exception ex)
            {
                var emptyVariant = new ProductVariantViewModel { TempId = 1 };
                ViewBag.VariationAttributes = new List<CategoryAttributeViewModel>();
                return PartialView("_VariantEditor", emptyVariant);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetMainImage(int imageId)
        {
            try
            {
                await _productRepository.SetMainImageAsync(imageId);
                return Json(new { success = true, message = "Ana resim başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            try
            {
                await _productRepository.DeleteImageAsync(imageId);
                return Json(new { success = true, message = "Resim başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string GetPlatformDisplayName(Platform platform)
        {
            return platform switch
            {
                Platform.Trendyol => "Trendyol",
                _ => "Bilinmeyen"
            };
        }
    }
}
