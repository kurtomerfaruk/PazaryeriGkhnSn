using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Identity.Client.Extensions.Msal;
using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Entity.Trendyol.CategoryAttribute;
using Pazaryeri.Entity.Trendyol.Orders;
using Pazaryeri.Entity.Trendyol.Products;
using Pazaryeri.Entity.Trendyol.Questions;
using Pazaryeri.Entity.Trendyol.Request;
using Pazaryeri.Entity.Trendyol.Response;
using Pazaryeri.Entity.Trendyol.Transaction;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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

        public async Task<BatchRequestIdResponse> UpdateStockPrice(List<StockPriceRequest> list)
        {
            try
            {
                var request = new RestRequest($"inventory/sellers/{_configuration["Trendyol:SupplierId"]}/products/price-and-inventory", Method.Post);

                var param = new
                {
                    items = list,
                };

                var json = JsonConvert.SerializeObject(param);

                request.AddJsonBody(json);

                var response = await _client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<BatchRequestIdResponse>(response.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol stok fiyat guncellenirken hata olustu");
                return new BatchRequestIdResponse();
            }
        }

        public async Task<StockPriceBatchResponse> UpdateStockPriceBatchResult(string batchRequestId)
        {
            try
            {
                var request = new RestRequest($"product/sellers/{_configuration["Trendyol:SupplierId"]}/products/batch-requests/{batchRequestId}", Method.Get);


                var response = await _client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<StockPriceBatchResponse>(response.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol stok fiyat guncellenirken hata olustu");
                return new StockPriceBatchResponse();
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

        public async Task<TrendyolCategoryAttributes> GetCategoryAttributesAsync(int categoryId)
        {
            try
            {
                var request = new RestRequest($"product/product-categories/{categoryId}/attributes");
                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful & !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<TrendyolCategoryAttributes>(response.Content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol Kategori Özellikleri sorgularken hata :", ex.Message);
            }
            return new TrendyolCategoryAttributes();
        }

        public async Task<List<Question>> GetQuestionsAsync(int page = 0, int size = 50)
        {
            return await GetAlQuestionsRecursiveAsync(page, size);
        }

        private async Task<List<Question>> GetAlQuestionsRecursiveAsync(int page = 0, int size = 50)
        {
            var allQuestion = new List<Question>();
            int currentPage = page;

            do
            {
                try
                {
                    var request = new RestRequest($"qna/sellers/{_configuration["Trendyol:SupplierId"]}/questions/filter");
                    request.AddParameter("page", currentPage);
                    request.AddParameter("size", size);
                    //request.AddParameter("status", "WAITING_FOR_ANSWER");
                    request.AddParameter("orderByField", "CreatedDate");
                    request.AddParameter("orderByDirection", "ASC");

                    var response = await _client.ExecuteAsync(request);

                    if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                    {
                        var trendyolResponse = JsonConvert.DeserializeObject<TrendyolQuestions>(response.Content);

                        if (trendyolResponse?.content != null && trendyolResponse.content.Any())
                        {
                            var questions = trendyolResponse.content.Select(CreateQuestionFromTrendyol).ToList();
                            allQuestion.AddRange(questions);

                            if (questions.Count < size) break;
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

            return allQuestion;
        }

        private Question CreateQuestionFromTrendyol(QuestionContent question)
        {
            return new Question
            {
                QuestionId = question.id,
                Answer = JsonConvert.SerializeObject(question.answer),
                AnsweredDateMessage = question.answeredDateMessage,
                CreationDate = Util.LongToDatetime(question.creationDate),
                CustomerId = question.customerId,
                ImageUrl = question.imageUrl,
                ProductName = question.productName,
                Public = question.@public,
                ShowUserName = question.showUserName,
                Status = Util.GetTrendyolQuestionStatus(question.status),
                Text = question.text,
                UserName = question.userName,
                WebUrl = question.webUrl,
                ProductMainId = question.productMainId,
                Platform = Platform.Trendyol,

            };
        }

        public async Task<HttpStatusCode> ReplyQuestion(int questionId, string answer)
        {
            try
            {
                var request = new RestRequest($"qna/sellers/{_configuration["Trendyol:SupplierId"]}/questions/{questionId}/answers", Method.Post);

                var param = new
                {
                    text = answer
                };

                var json = JsonConvert.SerializeObject(param);

                request.AddJsonBody(json);

                var response = await _client.ExecuteAsync(request);
                return response.StatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol stok fiyat guncellenirken hata olustu");
                return HttpStatusCode.BadRequest;
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int page = 0, int size = 1000)
        {
            return await GetAllransactionsRecursiveAsync(page, size);
        }

        private async Task<List<Transaction>> GetAllransactionsRecursiveAsync(int page = 0, int size = 1000)
        {
            var allTransactions = new List<Transaction>();
            int currentPage = page;
            var transactionTypes = new List<string> { "PaymentOrder", "Stoppage", "CashAdvance" };

            foreach (var type in transactionTypes)
            {
                do
                {
                    try
                    {
                        var request = new RestRequest($"finance/che/sellers/{_configuration["Trendyol:SupplierId"]}/otherfinancials");
                        request.AddParameter("page", currentPage);
                        request.AddParameter("size", size);
                        request.AddParameter("transactionType", type);
                        request.AddParameter("startDate", Util.DateTimeToLong(DateTime.Now.AddDays(-14)));
                        request.AddParameter("endDate", Util.DateTimeToLong(DateTime.Now));

                        var response = await _client.ExecuteAsync(request);

                        if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                        {
                            var trendyolResponse = JsonConvert.DeserializeObject<TrendyolTransactions>(response.Content);

                            if (trendyolResponse?.content != null && trendyolResponse.content.Any())
                            {
                                var orders = trendyolResponse.content.Select(CreateTransactionFromTrendyol).ToList();
                                allTransactions.AddRange(orders);

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

            }

            return allTransactions;
        }

        private Transaction CreateTransactionFromTrendyol(TransactionContent content)
        {
            Transaction transaction = new Transaction();
            transaction.TransactionId = content.id;
            transaction.TransactionDate = Util.LongToDatetime(content.transactionDate);
            transaction.Barcode = content.barcode;
            transaction.TransactionType = content.transactionType;
            transaction.ReceiptId = content.receiptId;
            transaction.Description = content.description;
            transaction.Debt = decimal.Parse(content.debt.ToString());
            transaction.Credit = decimal.Parse(content.credit.ToString());
            transaction.PaymentPeriod = content.paymentPeriod;
            transaction.CommissionRate = content.commissionRate != null ? decimal.Parse(content.commissionRate.ToString()) : decimal.Parse("0");
            transaction.CommissionAmount = content.commissionAmount != null ? decimal.Parse(content.commissionAmount.ToString()) : decimal.Parse("0");
            transaction.CommissionInvoiceSerialNumber = content.commissionInvoiceSerialNumber;
            transaction.SellerRevenue = content.sellerRevenue != null ? decimal.Parse(content.sellerRevenue.ToString()) : decimal.Parse("0");
            transaction.OrderNumber = content.orderNumber;
            transaction.OrderDate = Util.LongToDatetime(content.orderDate);
            transaction.PaymentOrderId = content.paymentOrderId;
            transaction.PaymentDate = Util.LongToDatetime(content.paymentDate);
            transaction.SellerId = content.sellerId;
            transaction.StoreId = content.storeId;
            transaction.StoreName = content.storeName;
            transaction.StoreAddress = content.storeAddress;
            transaction.Country = content.country;
            transaction.Currency = content.currency;
            transaction.Affiliate = content.affiliate;
            transaction.ShipmentPackageId = content.shipmentPackageId;
            transaction.Platform = Platform.Trendyol;
            return transaction;
            //return new Transaction
            //{
            //    TransactionId = content.id,
            //    TransactionDate = Util.LongToDatetime(content.transactionDate),
            //    Barcode = content.barcode,
            //    TransactionType = content.transactionType,
            //    ReceiptId = content.receiptId,
            //    Description = content.description,
            //    Debt = decimal.Parse(content.debt.ToString()),
            //    Credit = decimal.Parse(content.credit.ToString()),
            //    PaymentPeriod = content.paymentPeriod,
            //    CommissionRate = decimal.Parse(content.commissionRate.ToString()),
            //    CommissionAmount = decimal.Parse(content.commissionAmount.ToString()),
            //    CommissionInvoiceSerialNumber = content.commissionInvoiceSerialNumber,
            //    SellerRevenue = decimal.Parse(content.sellerRevenue.ToString()),
            //    OrderNumber = content.orderNumber,
            //    OrderDate = Util.LongToDatetime(content.orderDate),
            //    PaymentOrderId = content.paymentOrderId,
            //    PaymentDate = Util.LongToDatetime(content.paymentDate),
            //    SellerId = content.sellerId,
            //    StoreId = content.storeId,
            //    StoreName = content.storeName,
            //    StoreAddress = content.storeAddress,
            //    Country = content.country,
            //    Currency = content.currency,
            //    Affiliate = content.affiliate,
            //    ShipmentPackageId = content.shipmentPackageId,
            //    Platform = Platform.Trendyol

            //};
        }
    }
}
