
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentGateway.Controllers;
using PaymentGateway.DTOs;
using PaymentGateway.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using PaymentGateway.Data;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.Helpers
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

         public async Task<bool> ProcessPaymentOrderAsync(Transaction transaction)
        {
            var _httpClient = _httpClientFactory.CreateClient();
            var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var paymentOrderRequest = $"{_apiSettings.base_api_url}/v1/payment-order/new-order";

            try
            {
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
                recipientItem.institutionIdentifier = "phone";

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

                var updateResult = await UpdateEntityPropertiesAsync(transaction, async entity =>
                {


                    entity.systemConversationId = paymentResponse.message.systemConversationId;
                    entity.originatorConversationId = paymentResponse.message.originatorConversationId;
                    entity.IsPosted = true;
                    return true;
                });

                if (updateResult)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }


    }
}
