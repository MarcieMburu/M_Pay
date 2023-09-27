using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGateway.Data;
using PaymentGateway.DTOs;
using PaymentGateway.Helpers;
using PaymentGateway.Models;
using System.Net.Http.Headers;
using System.Text;

namespace WorkerService
{
    public class PaymentOrderRequest : BackgroundService
    {
        private readonly ILogger<PaymentOrderRequest> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly HttpClient _httpClient;
        private AccessToken _accessToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private ApiSettings _apiSettings;
        private readonly string client_id;
        private readonly string client_secret;
        private readonly string base_api_url;
        private IRepository<Transaction> _repository;
        private readonly IBus _bus;
        private readonly IRequestClient<PaymentMessage> _requestClient;
        private readonly IBusControl _busControl;
        private readonly IPublishEndpoint _publishEndpoint;



        public PaymentOrderRequest(ILogger<PaymentOrderRequest> logger, IServiceScopeFactory serviceScopeFactory,
            HttpClient httpClient, IHttpClientFactory httpClientFactory, IMapper mapper, IOptions<ApiSettings> apiSettings,
            IServiceProvider serviceProvider, IBus bus, IBusControl busControl)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _bus = bus;
            _serviceProvider = serviceProvider;
            client_id = "rahab";
            client_secret = "rahab";
            base_api_url = "https://sandboxapi.zamupay.com";
            _busControl = busControl;
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
            catch (Exception)
            {

            }

            return null;
        }
        public async Task PublishPaymentMessageAsync(PaymentMessage paymentMessage)
        {
            await _publishEndpoint.Publish(paymentMessage);

            //await _bus.Publish(paymentMessage);
        }
       

      
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        
        {
            _logger.LogInformation("Posting  bus started.");

            await _busControl.StartAsync(stoppingToken);

            var _httpClient = _httpClientFactory.CreateClient();
            var accessToken = await GetBearerToken(client_id, client_secret);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var paymentOrderRequest = $"{base_api_url}/v1/payment-order/new-order";

            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation("Posting saved transactions to Zamupay API...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedContext = scope.ServiceProvider.GetRequiredService<PaymentGatewayContext>();
                    var httpClient = _httpClientFactory.CreateClient();
                    var requestClient = scope.ServiceProvider.GetRequiredService<IRequestClient<PaymentMessage>>();



                    var orderLines = scopedContext.Transaction
                 .Where(t => !t.IsPosted)
                 .ToList();

                    foreach (var transaction in orderLines)
                    
                    {

                        if (transaction.IsPosted)
                        {

                            _logger.LogInformation($"Transaction with ID {transaction.Id} has already been posted. Skipping post");
                            continue;
                        }

                        //var paymentMessageResponse = new PaymentMessage
                        //{
                        //    Id = transaction.Id,
                        //    SenderName = transaction.Sender.Name,
                        //    SenderID_NO = transaction.Sender.ID_NO,
                        //    SenderPhone_No = transaction.Sender.Phone_No,
                        //    SenderSrc_Account = transaction.Sender.Src_Account,
                        //    ReceiverName = transaction.Receiver.Name,
                        //    ReceiverID_NO = transaction.Receiver.ID_NO,
                        //    originatorConversationId = transaction.originatorConversationId,
                            
                        //    Amount = transaction.Amount,

                        //};



                        _logger.LogInformation("Received Payment Message...");

                  //      await _busControl.StartAsync(stoppingToken);




                        // var responseMessage = await requestClient.GetResponse<PaymentMessage>(paymentMessageResponse, timeout: TimeSpan.FromSeconds(60));

                        //await _publishEndpoint.Publish(paymentMessageResponse);

                        //try { 


                        //var paymentResponse = responseMessage.Message;
                        //    _logger.LogInformation("Received Payment Response: " + paymentResponse);
                        //}
                        //catch (RequestTimeoutException)
                        //{
                        //    _logger.LogError("Payment Request Timed Out.");
                        //}
                        //catch (Exception ex)
                        //{
                        //    _logger.LogError("An error occurred: " + ex.Message);
                        //}


                        ZamupayRequest orderRequest = new ZamupayRequest();

                        RecipientItem recipientItem = new RecipientItem();
                        RemitterItem remitterItem = new RemitterItem();
                        TransactionItem transactionItem = new TransactionItem();

                        remitterItem.name = transaction.Sender.Name;
                        remitterItem.idNumber = transaction.Sender.ID_NO;
                        remitterItem.phoneNumber = transaction.Sender.Phone_No;
                        remitterItem.address = "nyeri";
                        remitterItem.sourceOfFunds = transaction.Sender.Src_Account;
                        remitterItem.ccy = 404;
                        remitterItem.country = "Kenya";
                        remitterItem.nationality = "Kenyan";
                        remitterItem.principalActivity = "Business";
                        remitterItem.financialInstitution = "Bank";
                        remitterItem.dateOfBirth = "01/01/2002";

                        recipientItem.mccmnc = "63902";
                        recipientItem.ccy = 404;
                        recipientItem.country = "KE";
                        recipientItem.name = transaction.Receiver.Name;
                        recipientItem.idNumber = transaction.Receiver.ID_NO;
                        recipientItem.primaryAccountNumber = transaction.Receiver.Phone_No;
                        recipientItem.financialInstitution = "Mpesa";
                        recipientItem.idType = "National ID";
                        recipientItem.purpose = "TEST";
                        recipientItem.institutionIdentifier = "Finance";

                        transactionItem.amount = transaction.Amount;
                        transactionItem.ChannelType = transaction.ChannelType;
                        transactionItem.routeId = transaction.RouteId;
                        transactionItem.reference = transaction.reference;
                        transactionItem.systemTraceAuditNumber = transaction.systemTraceAuditNumber;

                        PaymentOrderLine paymentOrderLines = new PaymentOrderLine();

                        paymentOrderLines.remitter = remitterItem;
                        paymentOrderLines.recipient = recipientItem;
                        paymentOrderLines.transaction = transactionItem;

                        orderRequest.paymentOrderLines = new List<PaymentOrderLine>
                     {
                        paymentOrderLines
                    };

                        orderRequest.paymentNotes = "Transactions";

                        orderRequest.originatorConversationId = transaction.originatorConversationId;


                        var jsonPayload = JsonConvert.SerializeObject(orderRequest);
                        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync(paymentOrderRequest, httpContent);

                        //response.EnsureSuccessStatusCode(); 


                        var responseContent = await response.Content.ReadAsStringAsync();

                        try
                        {
                            response.EnsureSuccessStatusCode();



                            var paymentResponse = JsonConvert.DeserializeObject<PaymentRequestResponse>(responseContent);

                            if (paymentResponse.message.originatorConversationId != transaction.originatorConversationId)
                            {
                                throw new InvalidOperationException("Received response with unexpected TransactionId.");
                            }


                            var existingTransaction = transaction;

                            var txn = scope.ServiceProvider.GetRequiredService<IRepository<Transaction>>();

                            var updateResult = await txn.UpdateEntityPropertiesAsync(transaction, async entity =>
                            {


                                entity.systemConversationId = paymentResponse.message.systemConversationId;
                                entity.originatorConversationId = paymentResponse.message.originatorConversationId;
                                entity.IsPosted = true;
                                return true;
                            });

                            if (updateResult)
                            {
                                _logger.LogInformation($"Transaction with ID {transaction.Id} successfully posted.");
                            }
                            else
                            {
                                _logger.LogError($"Failed to update transaction with ID {transaction.Id} in the database.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to post transaction with ID {transaction.Id} to API: {ex.Message}");
                        }

                        //      await _bus.Publish(new PaymentMessage {               });
                  //      await _publishEndpoint.Publish(paymentMessageResponse);


                    }


                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);

                }
                _logger.LogInformation("Posting  worker stopped.");

            }




        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopped Rabbit Mq Server");

            return Task.WhenAll( base.StopAsync(stoppingToken), _busControl.StopAsync(stoppingToken));
            
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
  

