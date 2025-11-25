using Microsoft.AspNetCore.Mvc;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class CategoryAttributeController : BaseController
    {
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;
        private readonly ILogger<BrandController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPlatformServiceFactory _platformServiceFactory;

        public CategoryAttributeController(ICategoryAttributeRepository categoryAttributeRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<BrandController> logger,
            IConfiguration configuration)
        {
            _categoryAttributeRepository = categoryAttributeRepository;
            _logger = logger;
            _configuration = configuration;
            _platformServiceFactory = platformServiceFactory;
        }

        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            SetActiveMenu("CategoryAttribute");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCategoryAttributes()
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

                var (categoryAttributes, totalCount) = await _categoryAttributeRepository.GetPagedCategoryAttributesAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = categoryAttributes.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        categoryId = o.CategoryId,
                        category = o.Category,
                        attributeId = o.AttributeId,
                        attributeName = o.AttributeName,
                        attributeValueId = o.AttributeValueId,
                        attributeValueName = o.AttributeValueName,
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
