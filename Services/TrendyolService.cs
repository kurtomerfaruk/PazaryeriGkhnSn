using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using RestSharp;
using System.Text;

namespace Pazaryeri.Services
{
    public class TrendyolService : IPlatformService
    {
        private readonly IConfiguration _configuration;
        private readonly RestClient _client;

        public string PlatformName => "Trendyol";

        public TrendyolService(IConfiguration configuration)
        {
            _configuration = configuration;
            var options = new RestClientOptions("https://api.trendyol.com/sapigw/")
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

        public async Task<List<Models.Order>> GetOrdersAsync()
        {
            try
            {
                var request = new RestRequest($"suppliers/{_configuration["Trendyol:SupplierId"]}/orders?page=0&size=200");

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var trendyolOrders = JsonConvert.DeserializeObject<TrendyolOrders>(response.Content);

                    if (trendyolOrders?.content != null)
                    {
                        var orders = new List<Order>();

                        foreach (var trendyolOrder in trendyolOrders.content)
                        {
                            var order = new Models.Order
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
                                Status = GetOrderStatus(trendyolOrder.status),
                                Platform = OrderPlatform.Trendyol,
                                GrossAmount = decimal.Parse(trendyolOrder.grossAmount.ToString()),
                                TotalDiscount = decimal.Parse(trendyolOrder.totalDiscount.ToString()),
                                TotalPrice = decimal.Parse(trendyolOrder.totalPrice.ToString()),
                                TrendyolDetails = CreateTrendyolDetail(trendyolOrder.lines)
                            };

                            orders.Add(order);
                        }
                        return orders;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Trendyol API hatası: {ex.Message}");
            }

            return new List<Models.Order>();
        }



        private string GetOrderStatus(string trendyolStatus)
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
                LineId =line.id
            }).ToList();

            
        }

    

    }
}
