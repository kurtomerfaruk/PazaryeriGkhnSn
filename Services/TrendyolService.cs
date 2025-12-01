using DocumentFormat.OpenXml.Drawing.Charts;
using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol;
using Pazaryeri.Entity.Trendyol.Categories;
using Pazaryeri.Entity.Trendyol.CategoryAttribute;
using Pazaryeri.Entity.Trendyol.Claims;
using Pazaryeri.Entity.Trendyol.Orders;
using Pazaryeri.Entity.Trendyol.Products;
using Pazaryeri.Entity.Trendyol.Questions;
using Pazaryeri.Entity.Trendyol.Request;
using Pazaryeri.Entity.Trendyol.Response;
using Pazaryeri.Entity.Trendyol.Transaction;
using Pazaryeri.Helper;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using RestSharp;
using System.Collections.Generic;
using System.Net;
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

        public async Task<List<Models.Order>> GetOrdersAsync(int page = 0, int size = 200)
        {
            return await GetAllOrdersRecursiveAsync(page, size);
        }

        private async Task<List<Models.Order>> GetAllOrdersRecursiveAsync(int page = 0, int size = 200)
        {
            var allOrders = new List<Models.Order>();
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

        public async Task<bool> UpdateOrderStatus(Models.Order order)
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

                string json = JsonConvert.SerializeObject(myObject, Newtonsoft.Json.Formatting.Indented);

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

        public async Task<BatchResponse> UpdateStockPriceBatchResult(string batchRequestId)
        {
            try
            {
                var request = new RestRequest($"product/sellers/{_configuration["Trendyol:SupplierId"]}/products/batch-requests/{batchRequestId}", Method.Get);


                var response = await _client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<BatchResponse>(response.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol stok fiyat guncellenirken hata olustu");
                return new BatchResponse();
            }
        }

        private Models.Order CreateOrderFromTrendyol(OrderContent trendyolOrder)
        {
            return new Models.Order
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

        public async Task<List<IGrouping<string, ProductContent>>> GetGroupedProductsAsync()
        {
            var allProducts = await GetAllProductsRecursiveAsync();

            return allProducts
                .Where(p => !string.IsNullOrEmpty(p.productMainId))
                .GroupBy(p => p.productMainId)
                .ToList();
        }

        private async Task<List<ProductContent>> GetAllProductsRecursiveAsync(int page = 0, int size = 50)
        {
            var allProducts = new List<ProductContent>();
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
                        var products = response.Data.content;
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
            return await GetAllTransactionsRecursiveAsync(page, size);
        }

        private async Task<List<Transaction>> GetAllTransactionsRecursiveAsync(int page = 0, int size = 1000)
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
                                var transactions = trendyolResponse.content.Select(CreateTransactionFromTrendyol).ToList();
                                allTransactions.AddRange(transactions);

                                if (transactions.Count < size) break;
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
        }

        public async Task<TrendyolResult> CreateProduct(Task<TrendyolProductItemsResponse> model)
        {
            var request = new RestRequest($"product/sellers/{_configuration["Trendyol:SupplierId"]}/products", Method.Post);

            var json = JsonConvert.SerializeObject(model.Result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            request.AddJsonBody(json);

            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<TrendyolErrorResponse>(response.Content);
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        Error = error
                    };
                }
                catch
                {
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        RawResponse = response.Content
                    };
                }
            }

            try
            {
                var success = JsonConvert.DeserializeObject<TrendyolSuccessResponse>(response.Content);
                return new TrendyolResult
                {
                    IsSuccess = true,
                    Success = success
                };
            }
            catch
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<TrendyolErrorResponse>(response.Content);
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        Error = error
                    };
                }
                catch
                {
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        RawResponse = response.Content
                    };
                }
            }
        }

        public async Task<TrendyolResult> UpdateProduct(Task<TrendyolProductItemsResponse> model)
        {
            var request = new RestRequest($"product/sellers/{_configuration["Trendyol:SupplierId"]}/products", Method.Put);

            var json = JsonConvert.SerializeObject(model.Result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            request.AddJsonBody(json);

            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<TrendyolErrorResponse>(response.Content);
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        Error = error
                    };
                }
                catch
                {
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        RawResponse = response.Content
                    };
                }
            }

            try
            {
                var success = JsonConvert.DeserializeObject<TrendyolSuccessResponse>(response.Content);
                return new TrendyolResult
                {
                    IsSuccess = true,
                    Success = success
                };
            }
            catch
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<TrendyolErrorResponse>(response.Content);
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        Error = error
                    };
                }
                catch
                {
                    return new TrendyolResult
                    {
                        IsSuccess = false,
                        RawResponse = response.Content
                    };
                }
            }
        }

        public async Task<BatchResponse> CreateProductBatchResult(string batchRequestId)
        {
            try
            {
                var request = new RestRequest($"product/sellers/{_configuration["Trendyol:SupplierId"]}/products/batch-requests/{batchRequestId}", Method.Get);


                var response = await _client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<BatchResponse>(response.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol eklenirken hata olustu");
                return new BatchResponse();
            }
        }

        public async Task<List<Claim>> GetClaimsAsync(int page = 0, int size = 100)
        {
            return await GetAllClaimsRecursiveAsync(page, size);
        }

        private async Task<List<Claim>> GetAllClaimsRecursiveAsync(int page = 0, int size = 100)
        {
            var allClaims = new List<Claim>();
            int currentPage = page;


            do
            {
                try
                {
                    var request = new RestRequest($"order/sellers/{_configuration["Trendyol:SupplierId"]}/claims");
                    request.AddParameter("page", currentPage);
                    request.AddParameter("size", size);
                    request.AddParameter("startDate", Util.DateTimeToLong(DateTime.Now.AddDays(-14)));
                    request.AddParameter("endDate", Util.DateTimeToLong(DateTime.Now));

                    var response = await _client.ExecuteAsync(request);

                    if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                    {
                        var trendyolResponse = JsonConvert.DeserializeObject<TrendyolClaims>(response.Content);

                        if (trendyolResponse?.content != null && trendyolResponse.content.Any())
                        {
                            var claims = trendyolResponse.content.Select(CreateClaimFromTrendyol).ToList();
                            allClaims.AddRange(claims);

                            if (claims.Count < size) break;
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


            return allClaims;
        }

        private Claim CreateClaimFromTrendyol(ClaimContent content)
        {
            return new Claim
            {
                TrendyolClaimId = content.id,
                OrderNumber = content.orderNumber,
                OrderDate = Util.LongToDatetime(content.orderDate),
                CustomerName = content.customerFirstName + " " + content.customerLastName,
                ClaimDate = Util.LongToDatetime(content.claimDate),
                CargoTrackingNumber = content.cargoTrackingNumber.ToString(),
                CargoName = content.cargoProviderName,
                OrderShipmentPackageId = content.orderShipmentPackageId.ToString(),
                LastModifiedDate = Util.LongToDatetime(content.lastModifiedDate),
                Trendyols = new List<TrendyolClaim>
                {
                    new TrendyolClaim
                    {
                        TrendyolClaimId= content.id,
                        Items = JsonConvert.SerializeObject(content.items)

                    }
                },
                Platform = Platform.Trendyol
            };
        }

        public async Task<bool> ClaimApproveAsync(string claimId, string claimItemId)
        {
            try
            {
                var request = new RestRequest($"order/sellers/{_configuration["Trendyol:SupplierId"]}/claims/{claimId}/items/approve", Method.Put);


                var linesList = new List<dynamic>();

                dynamic myObject = new
                {
                    claimLineItemIdList = new List<string> { claimItemId },
                    @params = new { },
                };

                string json = JsonConvert.SerializeObject(myObject, Newtonsoft.Json.Formatting.Indented);

                request.AddJsonBody(json);

                var response = await _client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trendyol claim update error: ");
                return false;
            }
        }
    }
}
