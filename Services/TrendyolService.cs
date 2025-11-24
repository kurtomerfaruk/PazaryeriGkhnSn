using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using RestSharp;
using System.Text;

namespace Pazaryeri.Services
{
    public class TrendyolService : BasePlatformService
    {
        private readonly RestClient _client;

        public override string PlatformName => "Trendyol";

        public TrendyolService(IConfiguration configuration, ILogger<TrendyolService> logger)
            : base(configuration, logger)
        {
            var options = new RestClientOptions("https://apigw.trendyol.com/integration/")
            {
                MaxTimeout = 30000,
                ThrowOnAnyError = false
            };

            _client = new RestClient(options);

            var apiKey = _configuration["Trendyol:ApiKey"];
            var apiSecret = _configuration["Trendyol:ApiSecret"];

            _client.AddDefaultHeader("Authorization",
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"))}");
        }

        public async Task<List<Order>> GetOrdersAsync(int page = 0, int size = 200)
        {
            return await GetAllOrdersRecursiveAsync(page, size);
        }

        public async Task<bool> UpdateOrderStatus(Order order)
        {
            try
            {
                var request = new RestRequest($"order/sellers/{_configuration["Trendyol:SupplierId"]}/shipment-packages/{order.OrderId}", Method.Put);
               

                var linesList = new List<dynamic>();


                foreach (var line in order.TrendyolDetails)
                {
                    linesList.Add(new
                    {
                        lineId = line.LineId,
                        quantity = line.Quantity
                    });
                }

                dynamic myObject = new
                {
                    lines = linesList,
                    @params = new { },
                    status =  "Picking" 
                };

                string json = JsonConvert.SerializeObject(myObject, Formatting.Indented);

                request.AddJsonBody(json);

                var response = await _client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol status update hatası: {OrderId}", order.OrderId);
                return false;
            }
        }

        

        public async Task<bool> UpdateStock(string merchantSku, int quantity)
        {
            try
            {
                var request = new RestRequest($"suppliers/{_configuration["Trendyol:SupplierId"]}/products/price-and-inventory", Method.Post);
                var stockUpdate = new
                {
                    items = new[]
                    {
                    new
                    {
                        barcode = merchantSku,
                        quantity = quantity
                    }
                }
                };

                request.AddJsonBody(stockUpdate);

                var response = await _client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol stock update hatası: {MerchantSku}", merchantSku);
                return false;
            }
        }

        private async Task<List<Order>> GetAllOrdersRecursiveAsync(int page = 0, int size = 200)
        {
            var allOrders = new List<Order>();
            int currentPage = page;

            do
            {
                try
                {
                    var request = new RestRequest($"order/sellers/{_configuration["Trendyol:SupplierId"]}/orders");
                    request.AddParameter("page", currentPage);
                    request.AddParameter("size", size);
                    request.AddParameter("orderByField", "CreatedDate");
                    request.AddParameter("orderByDirection", "DESC");

                    var response = await _client.ExecuteAsync(request);

                    if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                    {
                        var trendyolResponse = JsonConvert.DeserializeObject<TrendyolOrders>(response.Content);

                        if (trendyolResponse?.content != null && trendyolResponse.content.Any())
                        {
                            var orders = trendyolResponse.content.Select(CreateOrderFromTrendyol).ToList();
                            allOrders.AddRange(orders);

                            if (orders.Count < size) break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    currentPage++;
                    await Task.Delay(100);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Trendyol sayfa {Page} çekilirken hata", currentPage);
                    break;
                }

            } while (currentPage < 1000);

            return allOrders;
        }

        private Order CreateOrderFromTrendyol(Content trendyolOrder)
        {
            return new Order
            {
                OrderId = trendyolOrder.id.ToString(),
                OrderNumber = trendyolOrder.orderNumber,
                OrderDate = Helper.Util.LongToDatetime(trendyolOrder.orderDate),
                CustomerName = $"{trendyolOrder.customerFirstName} {trendyolOrder.customerLastName}",
                CustomerPhone = trendyolOrder.shipmentAddress.phone,
                CustomerEmail = trendyolOrder.customerEmail,
                CustomerAddress = trendyolOrder.shipmentAddress.fullAddress ?? trendyolOrder.shipmentAddress.address1,
                BillName = trendyolOrder.invoiceAddress.fullName,
                BillPhone = trendyolOrder.invoiceAddress.phone,
                BillAddress = trendyolOrder.invoiceAddress.fullAddress ?? trendyolOrder.invoiceAddress.address1,
                BillDistrict = trendyolOrder.invoiceAddress.district,
                BillCity = trendyolOrder.invoiceAddress.city,
                TaxNumber = trendyolOrder.identityNumber,
                Status = GetTrendyolOrderStatus(trendyolOrder.status),
                Platform = OrderPlatform.Trendyol,
                GrossAmount = decimal.Parse(trendyolOrder.grossAmount.ToString()),
                TotalDiscount = decimal.Parse(trendyolOrder.totalDiscount.ToString()),
                TotalPrice = decimal.Parse(trendyolOrder.totalPrice.ToString()),
                TrendyolDetails = CreateTrendyolDetail(trendyolOrder.lines)
            };
        }

        private List<TrendyolOrderDetail> CreateTrendyolDetail(IList<Line> lines)
        {
            if (lines == null || !lines.Any())
                return new List<TrendyolOrderDetail>();

            return lines.Select(line => new TrendyolOrderDetail
            {
                ProductSize = line.productSize,
                Sku = line.sku,
                MerchantSku = line.merchantSku,
                ProductName = line.productName,
                Barcode = line.barcode,
                Quantity = line.quantity,
                Amount = decimal.Parse(line.amount.ToString()),
                Discount = decimal.Parse(line.discount.ToString()),
                Price = decimal.Parse(line.price.ToString()),
                TotalPrice = decimal.Parse((line.quantity * line.price).ToString()),
                LineId = line.id
            }).ToList();


        }

        private string GetTrendyolOrderStatus(string trendyolStatus)
        {
            return trendyolStatus switch
            {
                "Created" => "Oluşturuldu",
                "Picking" => "Hazırlanıyor",
                "Invoiced" => "Faturalandı",
                "Shipped" => "Kargoda",
                "Delivered" => "Teslim Edildi",
                "Cancelled" => "İptal Edildi",
                "Returned" => "İade Edildi",
                _ => "Bekliyor"
            };
        }
    }
}
