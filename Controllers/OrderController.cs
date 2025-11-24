using Microsoft.AspNetCore.Mvc;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using Pazaryeri.Repositories;
using Pazaryeri.Services;

namespace Pazaryeri.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPlatformServiceFactory _platformServiceFactory;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderSyncService _orderSyncService;
        private readonly IConfiguration _configuration;

        public OrderController(IOrderRepository orderRepository, IPlatformServiceFactory platformServiceFactory, ILogger<OrderController> logger, IOrderSyncService orderSyncService, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _platformServiceFactory = platformServiceFactory;
            _logger = logger;
            _orderSyncService = orderSyncService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var platforms = _platformServiceFactory.GetAvailablePlatforms();
            ViewBag.Platforms = platforms;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetOrders()
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

                var (orders, totalCount) = await _orderRepository.GetPagedOrdersAsync(skip, pageSize, searchValue, sortColumn, sortDirection);

                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = orders.Select(o => new
                    {
                        id = o.Id,
                        platform = GetPlatformDisplayName(o.Platform),
                        orderDate = o.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        orderNumber = o.OrderNumber,
                        customerName = o.CustomerName,
                        totalPrice = o.TotalPrice.ToString("C2"),
                        status = o.Status,
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

        private string GetPlatformDisplayName(OrderPlatform platform)
        {
            return platform switch
            {
                OrderPlatform.Trendyol => "Trendyol",
                _ => "Bilinmeyen"
            };
        }


        [HttpPost]
        public async Task<IActionResult> FetchPlatformOrders(string platform)
        {
            try
            {
                var platformService = _platformServiceFactory.GetService(platform);
                var platformOrders = await platformService.GetOrdersAsync();

                int addedCount = 0;
                foreach (var order in platformOrders)
                {
                    if (!await _orderRepository.OrderExistsAsync(order.OrderNumber, order.Platform))
                    {
                        await _orderRepository.CreateAsync(order);
                        addedCount++;
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"{platformService.PlatformName} için {addedCount} yeni sipariş eklendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"{platform} siparişleri çekilirken hata: {ex.Message}"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> FetchAllPlatformOrders()
        {
            try
            {
                var platforms = _platformServiceFactory.GetAvailablePlatforms();
                var results = new List<object>();
                int totalAdded = 0;

                foreach (var platform in platforms)
                {
                    var platformService = _platformServiceFactory.GetService(platform);
                    var platformOrders = await platformService.GetOrdersAsync();

                    int platformAdded = 0;
                    foreach (var order in platformOrders)
                    {
                        if (!await _orderRepository.OrderExistsAsync(order.OrderNumber, order.Platform))
                        {
                            await _orderRepository.CreateAsync(order);
                            platformAdded++;
                            totalAdded++;
                        }
                    }

                    results.Add(new
                    {
                        platform = platformService.PlatformName,
                        added = platformAdded,
                        total = platformOrders.Count
                    });
                }

                return Json(new
                {
                    success = true,
                    message = $"{totalAdded} yeni sipariş eklendi.",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Siparişler çekilirken hata: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            try
            {
                var order = await _orderRepository.GetWithDetailsAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Sipariş bulunamadı" });
                }

                var result = new
                {
                    success = true,
                    order = new
                    {
                        id = order.Id,
                        orderNumber = order.OrderNumber,
                        customerName = order.CustomerName,
                        customerEmail = order.CustomerEmail,
                        totalPrice = order.TotalPrice.ToString("C2"),
                        status = order.Status,
                        platform = order.Platform.ToString(),
                        orderDate = order.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                    },
                    items = order.TrendyolDetails.Select(oi => new
                    {
                        productName = oi.ProductName,
                        barcode = oi.Barcode,
                        price = oi.Price.ToString("C2"),
                        quantity = oi.Quantity,
                        totalPrice = oi.TotalPrice.ToString("C2"),
                    }),
                    addresses = new
                    {
                        invoice = order.BillAddress,
                        shipment = order.CustomerAddress
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ManualSync()
        {
            try
            {
                _logger.LogInformation("Manuel senkronizasyon başlatıldı");

                await _orderSyncService.SyncOrdersAsync();

                return Json(new
                {
                    success = true,
                    message = "Manuel senkronizasyon başarıyla tamamlandı",
                    time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manuel senkronizasyon sırasında hata oluştu");
                return Json(new
                {
                    success = false,
                    message = $"Senkronizasyon hatası: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public IActionResult SyncStatus()
        {
            var intervalMinutes = _configuration.GetValue<int>("SyncSettings:IntervalMinutes", 5);
            // Son senkronizasyon durumunu döndürebilirsiniz
            return Json(new
            {
                lastSync = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                nextSync = DateTime.Now.AddMinutes(intervalMinutes).ToString("dd.MM.yyyy HH:mm:ss"),
                isRunning = true
            });
        }

    }
}
