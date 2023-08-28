using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGateway.Data;
using PaymentGateway.DTOs;
using PaymentGateway.Helpers;
using PaymentGateway.Models;
using System.Net.Http.Headers;

namespace WorkerService
{
    public class GetTransactionStatus : BackgroundService
    {
        private readonly ILogger<GetTransactionStatus> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
               private readonly HttpClient _httpClient;
               private AccessToken _accessToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private ApiSettings _apiSettings;

        public GetTransactionStatus(ILogger<GetTransactionStatus> logger, IServiceScopeFactory serviceScopeFactory, 
            HttpClient httpClient ,IHttpClientFactory httpClientFactory, IMapper mapper, IOptions<ApiSettings> apiSettings,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetBearerToken(string client_id, string client_secret)
        {

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",_apiSettings.client_id),
                new KeyValuePair<string, string>("client_secret",_apiSettings.client_secret),
                 new KeyValuePair<string, string>("grant_type",_apiSettings.grant_type),
                 new KeyValuePair<string, string>("scope",_apiSettings.scope)
        });

            try
            {
                var response = await _httpClient.PostAsync($"{_apiSettings.base_token_url}/connect/token", requestContent);
                //response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var accessToken = JsonConvert.DeserializeObject<AccessToken>(responseContent);


                return accessToken.access_token;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _httpClient = _httpClientFactory.CreateClient();
            var accessToken = await GetBearerToken(_apiSettings.client_id,_apiSettings.client_secret);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

         
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Transaction status update worker is running...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedContext = scope.ServiceProvider.GetRequiredService<PaymentGatewayContext>();
                    var httpClient = _httpClientFactory.CreateClient();

                    var orderLines = scopedContext.Transaction
                 .Where(t => !t.IsStatusUpdated) 
                 .ToList();


                    foreach (var transaction in orderLines)
                    {
                        if (!transaction.IsPosted)
                        {
                            _logger.LogInformation($"Transaction with ID {transaction.Id} has not been posted yet. Skipping status check.");
                            continue;
                        }

                        var apiUrl = $"{_apiSettings.base_api_url}/v1/payment-order/check-status?Id={transaction.originatorConversationId}&IdType=OriginatorConversationId";

                        _logger.LogInformation($"Request URL: {apiUrl}");


                        var response = await _httpClient.GetAsync(apiUrl);
                        var responseString = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();

                            var statusResponse = JsonConvert.DeserializeObject<PaymentDetails>(responseContent);

                            var txn = scope.ServiceProvider.GetRequiredService<IRepository<Transaction>>();



                          
                            if (statusResponse.originatorConversationId != transaction.originatorConversationId)
                            {
                                throw new InvalidOperationException("Received response with unexpected TransactionId.");
                            }

                            var updateResult = await txn.UpdateEntityPropertiesAsync(transaction, async entity =>
                            {


                                entity.transactionStatus = statusResponse.orderLines[0].transactionOutcome.transactionStatus;
                                entity.transactionStatusDescription = statusResponse.orderLines[0].transactionOutcome.transactionStatusDescription;
                                entity.transactionStatusDescription = statusResponse.orderLines[0].transactionOutcome.resultCode;
                                entity.transactionStatusDescription = statusResponse.orderLines[0].transactionOutcome.resultCodeDescription;
                                entity.IsStatusUpdated = true;
                                return true;
                            });


                           await scopedContext.SaveChangesAsync();
                            _logger.LogInformation($"Transaction status updated for ID: {transaction.Id}");
                        }
                        else
                        {
                            _logger.LogError($"Failed to fetch status for transaction ID: {transaction.Id}");
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
            _logger.LogInformation("Transaction status updater worker stopped.");

        }




        public class ApiSettings
        {
            public string client_id { get; set; }

            public string client_secret { get; set; }
            public string base_api_url { get; set; }
            public string base_token_url { get; set; }
            public string grant_type { get; set; }

            public string scope { get; set; }

        }
    }
  
}