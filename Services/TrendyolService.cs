using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Entity.Trendyol.Orders;
using Pazaryeri.Entity.Trendyol.Products;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using RestSharp;
using System.Collections.Generic;
using System.Net.Http;
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
                ThrowOnAnyError = false
            };

            _client = new RestClient(options);

            var apiKey = _configuration["Trendyol:ApiKey"];
            var apiSecret = _configuration["Trendyol:ApiSecret"];
            var supplierId = _configuration["Trendyol:SupplierId"];

            _client.AddDefaultHeader("Authorization",
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"))}");
            _client.AddDefaultHeader("User-Agent", $"{supplierId} - SelfIntegration");
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
                    status = "Picking"
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

        private Order CreateOrderFromTrendyol(OrderContent trendyolOrder)
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
                Platform = Platform.Trendyol,
                GrossAmount = decimal.Parse(trendyolOrder.grossAmount.ToString()),
                TotalDiscount = decimal.Parse(trendyolOrder.totalDiscount.ToString()),
                TotalPrice = decimal.Parse(trendyolOrder.totalPrice.ToString()),
                TrendyolDetails = CreateTrendyolOrderDetail(trendyolOrder.lines)
            };
        }

        private List<TrendyolOrderDetail> CreateTrendyolOrderDetail(IList<Line> lines)
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

        public async Task<List<IGrouping<string, TrendyolProductDetail>>> GetGroupedProductsAsync()
        {
            var allProducts = await GetAllProductsRecursiveAsync();

            return allProducts
                .Where(p => !string.IsNullOrEmpty(p.ProductMainId))
                .GroupBy(p => p.ProductMainId)
                .ToList();
        }

        private async Task<List<TrendyolProductDetail>> GetAllProductsRecursiveAsync(int page = 0, int size = 50)
        {
            var allProducts = new List<TrendyolProductDetail>();
            int currentPage = page;

            do
            {
                try
                {
                    var request = new RestRequest($"/product/sellers/{_configuration["Trendyol:SupplierId"]}/products");
                    request.AddParameter("page", currentPage);
                    request.AddParameter("size", size);

                    var response = await _client.ExecuteAsync<TrendyolProducts>(request);

                    if (response.IsSuccessful && response.Data?.content != null && response.Data.content.Any())
                    {
                        var products = response.Data.content.Select(CreateProductFromTrendyol).ToList();
                        allProducts.AddRange(products);
                      
                        if (products.Count < size) break;
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
                    _logger.LogError(ex, "Trendyol ürün sayfa {Page} çekilirken hata", currentPage);
                    break;
                }

            } while (currentPage < 1000);

            return allProducts;
        }

        private TrendyolProductDetail CreateProductFromTrendyol(ProductContent trendyolProduct)
        {
            var color = trendyolProduct.attributes?
                .FirstOrDefault(a => a.attributeName == "Renk")?.attributeValue;

            var webColor = trendyolProduct.attributes?
                .FirstOrDefault(a => a.attributeName == "Web Color")?.attributeValue;
            var subtitle = trendyolProduct.title?.Length > 50
        ? trendyolProduct.title.Substring(0, 50)
        : trendyolProduct.title;

            List<TrendyolAttribute> attributes = trendyolProduct.attributes?
                                    .Select(item => new TrendyolAttribute
                                    {
                                        AttributeId = item.attributeId,
                                        AttributeName = item.attributeName,
                                        AttributeValue = item.attributeValue,
                                        AttributeValueId = item.attributeValueId,
                                    })
                                    .ToList() ?? new List<TrendyolAttribute>();

            List<TrendyolImage> images = trendyolProduct.images?
                                   .Select(item => new TrendyolImage
                                   {
                                       Url = item.url
                                   })
                                   .ToList() ?? new List<TrendyolImage>();


            return new TrendyolProductDetail
            {
                ProductMainId = trendyolProduct.productMainId,
                Title = trendyolProduct.title,
                Subtitle = subtitle,
                Description = trendyolProduct.description,
                Barcode = trendyolProduct.barcode,
                TrenyolProductId = trendyolProduct.id,
                BrandId = trendyolProduct.brandId,
                CategoryId = trendyolProduct.pimCategoryId,
                Quantity = trendyolProduct.quantity,
                StockCode = trendyolProduct.stockCode,
                DimensionalWeight = trendyolProduct.dimensionalWeight,
                ListPrice = decimal.Parse(trendyolProduct.listPrice.ToString()),
                SalePrice = decimal.Parse(trendyolProduct.salePrice.ToString()),
                VatRate = trendyolProduct.vatRate,
                ProductCode = trendyolProduct.productCode,
                ProductUrl = trendyolProduct.productUrl,
                SaleStatus = trendyolProduct.onSale,
                ApprovalStatus = trendyolProduct.approved,
                Attributes = attributes,
                Images = images,
            };
        }

        public async Task<List<TrendyolBrand>> GetBrandsAsync(string value)
        {
            try
            {
                var request = new RestRequest($"product/brands/by-name");
                request.AddParameter("name", value);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<List<TrendyolBrand>>(response.Content);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol marka sorgulanirken hata", ex.Message);
            }

            return new List<TrendyolBrand>();
        }

        public async Task<TrendyolCategories> GetCategoriesAsync()
        {
            try
            {
                var request = new RestRequest($"product/product-categories");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<TrendyolCategories>(response.Content);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol kategori sorgulanirken hata", ex.Message);
            }

            return new TrendyolCategories();
        }

    }
}
