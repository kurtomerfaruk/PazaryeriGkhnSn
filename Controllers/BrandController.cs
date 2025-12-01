using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class BrandController : BaseController
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<BrandController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPlatformServiceFactory _platformServiceFactory;

        public BrandController(IBrandRepository brandRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<BrandController> logger,
            IConfiguration configuration)
        {
            _brandRepository = brandRepository;
            _logger = logger;
            _configuration = configuration;
            _platformServiceFactory = platformServiceFactory;
        }
        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            SetActiveMenu("Brand");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetBrands()
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

                var (brands, totalCount) = await _brandRepository.GetPagedAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = brands.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        brandId = o.BrandId,
                        name = o.Name,
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
        public async Task<IActionResult> FetchTrendyolBrands(string search)
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                List<TrendyolBrand> brands = await trendyolService.GetBrandsAsync(search);
                int addedCount = 0;
                int updatedCount = 0;
                foreach (var brand in brands)
                {
                    var ord = await _brandRepository.BrandExistsAsync(brand.name);

                    if (ord != null)
                    {
                        await _brandRepository.UpdateAsync(ord);
                        updatedCount++;
                    }
                    else
                    {
                        Brand brn = new Brand
                        {
                            BrandId =brand.id,
                            Name =brand.name,
                            Platform=Platform.Trendyol
                        };
                        await _brandRepository.CreateAsync(brn);
                        addedCount++;
                    }
                }
                return Json(new
                {
                    success = true,
                    message = $"Trendyol için {addedCount} yeni marka eklendi. {updatedCount} marka güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol markaları çekilirken hata: {ex.Message}"
                });
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
