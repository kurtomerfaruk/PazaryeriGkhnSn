using Newtonsoft.Json;
using Pazaryeri.Entity.Trendyol;
using RestSharp;

namespace Pazaryeri.Services.Api
{
    public class TrendyolApiService
    {
        private const string BaseUrl = "https://api.trendyol.com/sapigw/";
        private string Base64String = "";
        private const string ApiKey = "ylIGRd6BIp4KlxW3VfvO";
        private const string ApiSecret = "ezAWyuzfBuGHiCnV1C5k";
        private const string MerchantId = "112430";

        public TrendyolApiService()
        {
            Base64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ApiKey + ":" + ApiSecret));
        }

        public async Task<TrendyolOrders> GetOrdersAsync()
        {
            var options = new RestClientOptions(BaseUrl);
            var client = new RestClient(options);
            var request = new RestRequest("/integration/order/sellers/" + MerchantId + "/orders?page=0&size=200", Method.Get);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", Base64String);
            RestResponse response = await client.ExecuteGetAsync<TrendyolOrders>(request);
            if(!response.IsSuccessful || response.Content == null)
            {
                 throw new Exception(response.ErrorMessage);
            }
            return JsonConvert.DeserializeObject<TrendyolOrders>(response.Content)!;
        }
    }
}
