
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace Shared
{ 

    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PaymentGatewayContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient = new HttpClient();
        private ApiSettings _apiSettings;
        private AccessToken _accessToken;
        private IDistributedCache _distributedCache;
        private readonly ILogger<Repository<TEntity>> _logger;


        public Repository(IHttpClientFactory httpClientFactory, PaymentGatewayContext context,
            IMapper mapper, IOptions<ApiSettings> apiSettings, HttpClient httpClient, IDistributedCache distributedCache, ILogger<Repository<TEntity>> logger)
        {
            _context = context;

            this._mapper = mapper;

            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;

            _httpClientFactory = httpClientFactory;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<bool> UpdateEntityPropertiesAsync<TEntity>(TEntity entity, Func<TEntity, Task<bool>> updateAction)
        {
            try
            {
                if (entity == null)
                {
                    throw new InvalidOperationException("Entity not found in the database.");
                }

                var success = await updateAction(entity);

                if (success)
                {
                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                }
                return success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> GetBearerToken(string client_id, string client_secret)
        {

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",_apiSettings.client_id),
                new KeyValuePair<string, string>("client_secret",_apiSettings.client_secret),
                 new KeyValuePair<string, string>("grant_type", _apiSettings.grant_type),
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

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            var cachedTransactions = await _distributedCache.GetStringAsync("CachedTransactions");
            if (cachedTransactions != null)
            {
                _logger.LogInformation("Transactions fetched from cache.");
                return JsonConvert.DeserializeObject<List<Transaction>>(cachedTransactions);
            }

            // If not in cache, fetch from the database
            var transactions = await _context.Transaction.ToListAsync();

            // Store transactions in cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _distributedCache.SetStringAsync("CachedTransactions", JsonConvert.SerializeObject(transactions), cacheOptions);

            _logger.LogInformation("Transactions fetched from database and cached.");

            return transactions;
        }

        public async Task<ActionResult<Transaction>> GetLatestTransaction()
        {
            // Try to get cached latest transaction first
            var cachedLatestTransaction = await _distributedCache.GetStringAsync("cached_latest_transaction");

            if (!string.IsNullOrEmpty(cachedLatestTransaction))
            {
                var latestTransaction = JsonConvert.DeserializeObject<Transaction>(cachedLatestTransaction);
                return latestTransaction;
            }
            else
            {
                // If not in cache, retrieve latest transaction data from the database
                var latestTransaction = await _context.Transaction.OrderByDescending(t => t.Date).FirstOrDefaultAsync();



                // Store latest transaction data in cache for future use
                await _distributedCache.SetStringAsync("CachedTransactions", JsonConvert.SerializeObject(latestTransaction));

                return latestTransaction;
            }
        }



        public async Task<bool> ProcessPaymentOrderAsync<TEntity>(Transaction transaction, Func<TEntity, ZamupayRequest> mapToZamupayRequest)
        {
            var _httpClient = _httpClientFactory.CreateClient();
            var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var paymentOrderRequest = $"{_apiSettings.base_api_url}/v1/payment-order/new-order";

            try
            {
                var orderRequest = MapTransactionToZamupayRequest(transaction);

                var jsonPayload = JsonConvert.SerializeObject(orderRequest);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(paymentOrderRequest, httpContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                var paymentResponse = JsonConvert.DeserializeObject<PaymentRequestResponse>(responseContent);

                if (paymentResponse.message.originatorConversationId != transaction.originatorConversationId)
                {
                    throw new InvalidOperationException("Received response with unexpected TransactionId.");
                }

                var existingTransaction = transaction;

                if (existingTransaction == null)
                {
                    throw new InvalidOperationException("Transaction not found in the database.");
                }

                existingTransaction.systemConversationId = paymentResponse.message.systemConversationId;
                existingTransaction.originatorConversationId = paymentResponse.message.originatorConversationId;
                existingTransaction.IsPosted = true;

                // Save changes to the database
                // Assuming you have a reference to your database context (_context)
                _context.Update(existingTransaction);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return false;
            }
        }


        public ZamupayRequest MapTransactionToZamupayRequest(Transaction transaction)
        {
            var orderRequest = new ZamupayRequest();

            var remitterItem = new RemitterItem
            {
                name = transaction.Sender.Name,
                idNumber = transaction.Sender.ID_NO,
                phoneNumber = transaction.Sender.Phone_No,
                address = "nyeri",
                sourceOfFunds = transaction.Sender.Src_Account,
                ccy = 404,
                country = "Kenya",
                nationality = "Kenyan",
                principalActivity = "Business",
                financialInstitution = "Bank",
                dateOfBirth = "01/01/2002"
            };

            var recipientItem = new RecipientItem
            {
                mccmnc = "63902",
                ccy = 404,
                country = "KE",
                name = transaction.Receiver.Name,
                idNumber = transaction.Receiver.ID_NO,
                primaryAccountNumber = transaction.Receiver.Phone_No,
                financialInstitution = "Mpesa",
                idType = "National ID",
                purpose = "TEST",
                institutionIdentifier = "phone"
            };
            var transactionItem = new TransactionItem
            {
                amount = transaction.Amount,
                ChannelType = transaction.ChannelType,
                routeId = transaction.RouteId,
                reference = transaction.reference,
                systemTraceAuditNumber = transaction.systemTraceAuditNumber
            };
            var paymentOrderLines = new PaymentOrderLine
            {
                remitter = remitterItem,
                recipient = recipientItem,
                transaction = transactionItem
            };
            orderRequest.paymentOrderLines = new List<PaymentOrderLine>
             {
               paymentOrderLines
              };

            orderRequest.paymentNotes = "Transactions";
            orderRequest.originatorConversationId = transaction.originatorConversationId;

            return orderRequest;
        }

    }
}
