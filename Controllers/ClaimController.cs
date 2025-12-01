using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Entity.Trendyol.Claims;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Repositories.Interfaces;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class ClaimController : BaseController
    {
        private readonly IClaimRepository _claimRepository;
        private readonly ILogger<ClaimController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPlatformServiceFactory _platformServiceFactory;

        public ClaimController(IClaimRepository claimRepository,
            IPlatformServiceFactory platformServiceFactory,
            ILogger<ClaimController> logger,
            IConfiguration configuration)
        {
            _claimRepository = claimRepository;
            _logger = logger;
            _configuration = configuration;
            _platformServiceFactory = platformServiceFactory;
        }
        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            SetActiveMenu("Claim");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetClaims()
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

                var (claims, totalCount) = await _claimRepository.GetPagedAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = claims.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        orderNumber = o.OrderNumber,
                        orderDate = o.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        customerName=o.CustomerName,
                        claimDate = o.ClaimDate.ToString("dd.MM.yyyy HH:mm"),
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
        public async Task<IActionResult> FetchTrendyolClaims()
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                var claims = await trendyolService.GetClaimsAsync();
               

                await _claimRepository.AddOrUpdateRangeAsync(claims);

                return Json(new
                {
                    success = true,
                    message = $"Trendyol Sipariş iadeleri güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol Sipariş iadeleri çekilirken hata: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TrendyolApproveClaim(int claimId,string lineId)
        {
            try
            {
                var claim = await _claimRepository.GetByIdAsync(claimId);

                if (claim == null)
                {
                    return Json(new { success = false, message = "Sipariş İadesi bulunamadı" });
                }

                var trendyolService = _platformServiceFactory.GetTrendyolService();
                var approve = await trendyolService.ClaimApproveAsync(claim.TrendyolClaimId,lineId);

                if (approve)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Trendyol Sipariş iadeleri güncellendi."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = $"Trendyol Sipariş iadeleri güncellenirken beklenmeyen hata olustu."
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol Sipariş iadeleri çekilirken hata: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClaimDetails(int id)
        {
            try
            {
                var claim = await _claimRepository.GetWithDetailsAsync(id);
                if (claim == null)
                {
                    return Json(new { success = false, message = "Sipariş İadesi bulunamadı" });
                }

                var result = new
                {
                    success = true,
                    claim = new
                    {
                        id=claim.Id,
                        trendyolClaimId = claim.TrendyolClaimId,
                        orderNumber = claim.OrderNumber,
                        orderDate= claim.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        customerName = claim.CustomerName,
                        claimDate = claim.ClaimDate.ToString("dd.MM.yyyy HH:mm"),
                        cargoTrackingNumber = claim.CargoTrackingNumber,
                        cargoName = claim.CargoName,
                        orderShipmentPackageId = claim.OrderShipmentPackageId,
                        lastModifiedDate= claim.LastModifiedDate.ToString("dd.MM.yyyy HH:mm"),
                    },
                    trendyols = claim.Trendyols.Select(c=>new
                    {
                        items = JsonConvert.DeserializeObject<List<ClaimItems>>(c.Items)
                    })
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
