using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol.Products;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class ProductController : BaseController
    {

        private readonly IProductRepository _productRepository;
        private readonly ITrendyolProductDetailRepository _trendyolProductDetailRepository;
        private readonly IPlatformServiceFactory _platformServiceFactory;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(IProductRepository productRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<ProductController> logger,
            IConfiguration configuration,
            ITrendyolProductDetailRepository trendyolProductDetailRepository)
        {
            _productRepository = productRepository;
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
                        platform = GetPlatformDisplayName(o.Platform),
                        title = o.Title,
                        subtitle = o.Subtitle,
                        productmainid = o.ProductMainId,
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
                        subtitle= product.Subtitle,
                        description = product.Description,
                        platform = product.Platform.ToString(),
                    },
                    items = product.TrendyolDetails.Select(oi => new
                    {
                        id=oi.Id,
                        barcode = oi.Barcode,
                        quantity=oi.Quantity
                    }),
                   
                };

                return Json(result);
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
