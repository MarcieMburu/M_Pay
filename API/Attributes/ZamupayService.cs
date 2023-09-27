using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using System.Text;

namespace API.Attributes
{
    public class ZamupayService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<ApiSettings> _apiSettings;

        public ZamupayService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings;
        }

        public async Task<HttpResponseMessage> SendPaymentOrderRequestAsync(Transaction transaction)
        {
            // Construct the request and send it to Zamupay API
            var apiUrl = $"{_apiSettings.Value.base_api_url}/v1/payment-order/new-order";
            var jsonPayload = JsonConvert.SerializeObject(transaction);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, httpContent);
            return response;
        }

    }
}
