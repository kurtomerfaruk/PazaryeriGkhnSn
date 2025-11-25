using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPlatformServiceFactory _platformServiceFactory;

        public CategoryController(ICategoryRepository categoryRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<CategoryController> logger,
            IConfiguration configuration)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
            _configuration = configuration;
            _platformServiceFactory = platformServiceFactory;
        }

        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            SetActiveMenu("Category");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCategories()
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

                var (categories, totalCount) = await _categoryRepository.GetPagedCategoryAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = categories.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        categoryId = o.CategoryId,
                        categoryCode = o.CategoryCode,
                        title = o.Title,
                        parentCategoryId = o.ParentCategoryId,
                        topCategory = o.TopCategory,
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
        public async Task<IActionResult> FetchTrendyolCategories()
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                TrendyolCategories categories = await trendyolService.GetCategoriesAsync();
                var allCategories = FlattenCategoriesRecursive(categories.categories);
                var existingCategoryIds = await _categoryRepository.GetExistingCategoryIdsAsync();
                int addedCount = 0;
                int updatedCount = 0;

                var newCategories = allCategories
                    .Where(c => !existingCategoryIds.Contains(c.id))
                    .ToList();

                if (!newCategories.Any())
                {
                    _logger.LogInformation("Eklenebilecek yeni kategori bulunamadı.");
                    return Json(new
                    {
                        success = false,
                        message = "Eklenebilecek yeni kategori bulunamadı."
                    });
                }

                var dbCategories = new List<Category>();

                foreach (var category in newCategories)
                {
                    var fullPath = GetCategoryFullPath(category, allCategories);
                    var dbCategory = new Category
                    {
                        CategoryId = category.id,
                        CategoryCode = category.id.ToString(),
                        Title = fullPath,
                        Platform = Platform.Trendyol,
                        ParentCategoryId = category.parentId.ToString(),
                        TopCategory = category.parentId == null
                    };
                    dbCategories.Add(dbCategory);
                }

                await _categoryRepository.BulkInsertAsync(dbCategories);
                addedCount = newCategories.Count();
                updatedCount = allCategories.Count() - addedCount;
                return Json(new
                {
                    success = true,
                    message = $"Trendyol için {addedCount} yeni kategori eklendi. {updatedCount} kategori güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol kategoriler çekilirken hata: {ex.Message}"
                });
            }
        }

        private string GetCategoryFullPath(TrendyolCategory category, List<TrendyolCategory> allCategories)
        {
            var pathParts = new List<string> { category.name };
            var currentCategory = category;

            while (currentCategory.parentId.HasValue)
            {
                var parent = allCategories.FirstOrDefault(c => c.id == currentCategory.parentId);
                if (parent != null)
                {
                    pathParts.Insert(0, parent.name);
                    currentCategory = parent;
                }
                else
                {
                    break;
                }
            }

            return string.Join(" > ", pathParts);
        }

        private List<TrendyolCategory> FlattenCategoriesRecursive(List<TrendyolCategory> categories)
        {
            var result = new List<TrendyolCategory>();

            foreach (var category in categories)
            {
                result.Add(category);
                if (category.subCategories != null && category.subCategories.Any())
                {
                    result.AddRange(FlattenCategoriesRecursive(category.subCategories));
                }
            }

            return result;
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
