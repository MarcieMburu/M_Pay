using AutoMapper;
using Newtonsoft.Json;
using PaymentGateway.Data;
using PaymentGateway.DTOs;
using PaymentGateway.Helpers;
using PaymentGateway.Models;
using System.Net.Http.Headers;
using static API.Controllers.TransactionsController;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
       
        private readonly HttpClient _httpClient;
        private readonly string client_id;
        private readonly string client_secret;
        private readonly string base_api_url;
        private AccessToken _accessToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;


        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, 
            HttpClient httpClient ,IHttpClientFactory httpClientFactory, IMapper mapper,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _httpClient = httpClient;
            client_id = "rahab";
            client_secret = "rahab";
            base_api_url = "https://sandboxapi.zamupay.com";
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetBearerToken(string client_id, string client_secret)
        {

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",client_id),
                new KeyValuePair<string, string>("client_secret",client_secret),
                 new KeyValuePair<string, string>("grant_type","client_credentials"),
                 new KeyValuePair<string, string>("scope","PyPay_api")
        });

            try
            {
                var response = await _httpClient.PostAsync("https://auth.zamupay.com/connect/token", requestContent);
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
            var accessToken = await GetBearerToken(client_id, client_secret);
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

                        var apiUrl = $"{base_api_url}/v1/payment-order/check-status?Id={transaction.originatorConversationId}&IdType=OriginatorConversationId";

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
                                entity.IsStatusUpdated = true;
                                return true;
                            });


                         //   await scopedContext.SaveChangesAsync();
                            _logger.LogInformation($"Transaction status updated for ID: {transaction.Id}");
                        }
                        else
                        {
                            _logger.LogError($"Failed to fetch status for transaction ID: {transaction.Id}");
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); // Delay before next iteration
                }
            _logger.LogInformation("Transaction status updater worker stopped.");

        }
    }
  
}