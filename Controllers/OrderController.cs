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
        private readonly IConfiguration _configuration;

        public OrderController(IOrderRepository orderRepository, IPlatformServiceFactory platformServiceFactory, ILogger<OrderController> logger, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _platformServiceFactory = platformServiceFactory;
            _logger = logger;
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
        public async Task<IActionResult> FetchTrendyolOrders()
        {
            try
            {
                var trendyolService = _platformServiceFactory.GetTrendyolService();
                List<Order> orders = await trendyolService.GetOrdersAsync();
                int addedCount = 0;
                int updatedCount = 0;
                foreach (var order in orders)
                {
                    var ord = await _orderRepository.OrderExistsAsync(order.OrderNumber, order.Platform);

                    if (ord!=null)
                    {
                        await _orderRepository.UpdateAsync(order);
                        updatedCount++;
                    }
                    else
                    {
                        await _orderRepository.CreateAsync(order);
                        addedCount++;
                    }
                }
                return Json(new
                {
                    success = true,
                    message = $"Trendyol için {addedCount} yeni sipariş eklendi. {updatedCount} sipariş güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Trendyol siparişleri çekilirken hata: {ex.Message}"
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
        public async Task<IActionResult> UpdateTrendyolStatus(int orderId, string status)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order?.Platform != OrderPlatform.Trendyol)
                {
                    return Json(new { success = false, message = "Sadece Trendyol siparişleri güncellenebilir" });
                }

                var trendyolService = _platformServiceFactory.GetTrendyolService();
                var result = await trendyolService.UpdateOrderStatus(order);

                if (result)
                {
                    order.Status = status;
                    await _orderRepository.UpdateAsync(order);
                }

                return Json(new { success = result, message = result ? "Durum güncellendi" : "Güncelleme başarısız" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
