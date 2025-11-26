using Azure;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Pazaryeri.Dtos;
using Pazaryeri.Entity.Trendyol.Products;
using Pazaryeri.Entity.Trendyol.Request;
using Pazaryeri.Entity.Trendyol.Response;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
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

            // Dosya boyutu kontrolü (max 5MB)
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


        //[HttpGet]
        //public async Task<IActionResult> Create()
        //{

        //    var model = new ProductViewModel();
        //    model.Variants = new List<ProductVariantViewModel>
        //    {
        //        new ProductVariantViewModel()
        //    };
        //    await LoadBrandsAsync(model);
        //    await LoadCategoriesAsync(model);
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(ProductViewModel model)
        //{
        //    // Dropdownları her zaman yükle
        //    await LoadBrandsAsync(model);
        //    await LoadCategoriesAsync(model);

        //    // Model geçerli ise kaydet
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var product = await _productRepository.CreateProductAsync(model);

        //            // Ürün resimleri
        //            if (model.ImageFiles?.Any() == true)
        //                await UploadProductImages(product.Id, model.ImageFiles);

        //            // Varyasyon resimleri
        //            foreach (var variant in model.Variants)
        //            {
        //                if (variant.ImageFiles?.Any() == true)
        //                    await UploadVariantImages(variant.Id, variant.ImageFiles);
        //            }

        //            TempData["Success"] = "Ürün başarıyla oluşturuldu.";
        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError("", $"Ürün oluşturulurken hata oluştu: {ex.Message}");
        //        }
        //    }

        //    // -----------------------------
        //    // ModelState invalid → tekrar formu doldur
        //    // -----------------------------

        //    if (model.CategoryId != null)
        //    {
        //        var categoryDto = await _categoryRepository.GetCategoryWithAttributesAsync(model.CategoryId.Value);

        //        model.CategoryAttributes = categoryDto.CategoryAttributes.Select(a => new CategoryAttributeViewModel
        //        {
        //            Id = a.Id,
        //            Name = a.Name,
        //            Varianter = a.Varianter,
        //            AllowCustom = a.AllowCustom,
        //            Required = a.Required,
        //            AllowMultipleAttributeValues = a.AllowMultipleAttributeValues,
        //            Values = a.Values.Select(v => new CategoryAttributeValueViewModel
        //            {
        //                Id = v.Id,
        //                Name = v.Name
        //            }).ToList()
        //        }).ToList();

        //        // Varyasyonları global JS array’e eklemek için
        //        // <script> kısmında existingVariants = @Html.Raw(Json.Serialize(Model.Variants));
        //    }

        //    // Varyasyonlar zaten POST ile geldi → model.Variants değişmeden View’a döner
        //    return View(model);
        //}

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel();
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
                    var product = await _productRepository.CreateProductAsync(model);

                    // Ürün resimleri
                    if (model.ImageFiles?.Any() == true)
                        await UploadProductImages(product.Id, model.ImageFiles);

                    // Varyasyon resimleri
                    foreach (var variant in model.Variants)
                    {
                        if (variant.ImageFiles?.Any() == true)
                            await UploadVariantImages(variant.Id, variant.ImageFiles);
                    }

                    TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ürün oluşturulurken hata oluştu: {ex.Message}");
                }
            }

            // ModelState invalid → kategori özelliklerini yeniden yükle
            if (model.CategoryId != null)
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

            return View(model);
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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Ürünü güncelle
                    var product = await _productRepository.UpdateProductAsync(model);

                    // Yeni resimleri yükle
                    if (model.ImageFiles?.Any() == true)
                    {
                        await UploadProductImages(product.Id, model.ImageFiles);
                    }

                    // Trendyol'da güncelle
                    //var trendyolResult = await _trendyolService.UpdateProductAsync(product);

                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ürün güncellenirken hata oluştu: {ex.Message}");
                }
            }

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
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
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
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Barcode = v.Barcode,
                    ExistingImages = v.VariantImages.Select(vi => vi.ImageUrl).ToList()
                }).ToList()
            };
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AddVariant([FromBody] ProductViewModel model)
        //{
        //    model.Variants ??= new List<ProductVariantViewModel>();
        //    model.Variants.Add(new ProductVariantViewModel());

        //    return PartialView("_VariantPartial", model);
        //}

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

            // Boş seçenek ekle
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
            // Örnek: TrendyolService ile çekiyoruz
            CategoryDto categoryDto = await _categoryRepository.GetCategoryWithAttributesAsync(categoryId);

            if (categoryDto == null) return NotFound();

            return Json(new
            {
                success = true,
                data = categoryDto
            });
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
