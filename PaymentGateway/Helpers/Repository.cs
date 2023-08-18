
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
     

        public Repository(IHttpClientFactory httpClientFactory, PaymentGatewayContext context,
            IMapper mapper, IOptions<ApiSettings> apiSettings, HttpClient httpClient)
        {
            _context = context;

            this._mapper = mapper;

            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;

            _httpClientFactory = httpClientFactory;
           
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

       
        //[HttpPost]
        //public async Task<TEntity> PaymentOrderRequest(int id, string originatorConversationId)
        //{
        //    var transaction = await _context.TEntity.FindAsync(id);
            
        //    if (transaction == null)
        //    {
        //        return null;
        //    }

        //    if (transaction.IsPosted)
        //    {
        //       // return Ok(new { isTransactionPosted = true });
        //    }
        //    else
        //    {
        //        var isPaymentOrderRequestSuccessful = await ProcessPaymentOrderAsync(transaction);

        //        if (isPaymentOrderRequestSuccessful)
        //        {
        //            var updateResult = await UpdateEntityPropertiesAsync(
        //         transaction,
        //        async entity =>
        //        {
        //            entity.IsPosted = true;
        //            return true;
        //        }
        //     );

        //            if (updateResult)
        //            {
                        
        //            }
        //            else
        //            {
        //              //  return Ok(new { Message = "ERROR" });
        //            }
        //        }
        //        else
        //        {
        //          //  return Ok(new { Message = "ERROR" });
        //        }
        //    }
        //    return transaction;
        //}

      

        //public  async Task<bool> ProcessPaymentOrderAsync<TEntity>(TEntity entity, Func<TEntity, Task<bool>> processAction)
        //{
        
        //var _httpClient = _httpClientFactory.CreateClient();
        //    var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //    var paymentOrderRequest = $"{_apiSettings.base_api_url}/v1/payment-order/new-order";

        //    try
        //    {
        //        ZamupayRequest orderRequest = new ZamupayRequest();

        //        RecipientItem recipientItem = new RecipientItem();
        //        RemitterItem remitterItem = new RemitterItem();
        //        TransactionItem transactionItem = new TransactionItem();

        //        remitterItem.name = entity.Sender.Name;
        //        remitterItem.idNumber = transaction.Sender.ID_NO;
        //        remitterItem.phoneNumber = transaction.Sender.Phone_No;
        //        remitterItem.address = "nyeri";
        //        remitterItem.sourceOfFunds = transaction.Sender.Src_Account;
        //        remitterItem.ccy = 404;
        //        remitterItem.country = "Kenya";
        //        remitterItem.nationality = "Kenyan";
        //        remitterItem.principalActivity = "Business";
        //        remitterItem.financialInstitution = "Bank";
        //        remitterItem.dateOfBirth = "01/01/2002";

        //        recipientItem.mccmnc = "63902";
        //        recipientItem.ccy = 404;
        //        recipientItem.country = "KE";
        //        recipientItem.name = transaction.Receiver.Name;
        //        recipientItem.idNumber = transaction.Receiver.ID_NO;
        //        recipientItem.primaryAccountNumber = transaction.Receiver.Phone_No;
        //        recipientItem.financialInstitution = "Mpesa";
        //        recipientItem.idType = "National ID";
        //        recipientItem.purpose = "TEST";

        //        transactionItem.amount = transaction.Amount;
        //        transactionItem.ChannelType = transaction.ChannelType;
        //        transactionItem.routeId = transaction.RouteId;
        //        transactionItem.reference = transaction.reference;
        //        transactionItem.systemTraceAuditNumber = transaction.systemTraceAuditNumber;

        //        PaymentOrderLine paymentOrderLines = new PaymentOrderLine();

        //        paymentOrderLines.remitter = remitterItem;
        //        paymentOrderLines.recipient = recipientItem;
        //        paymentOrderLines.transaction = transactionItem;

        //        orderRequest.paymentOrderLines = new List<PaymentOrderLine>
        //         {
        //            paymentOrderLines
        //        };

        //        orderRequest.paymentNotes = "Transactions";

        //        orderRequest.originatorConversationId = transaction.originatorConversationId;


        //        var jsonPayload = JsonConvert.SerializeObject(orderRequest);
        //        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


        //        var response = await _httpClient.PostAsync(paymentOrderRequest, httpContent);
        //        //response.EnsureSuccessStatusCode(); 


        //        var responseContent = await response.Content.ReadAsStringAsync();


        //        var paymentResponse = JsonConvert.DeserializeObject<PaymentRequestResponse>(responseContent);

        //        if (paymentResponse.message.originatorConversationId != transaction.originatorConversationId)
        //        {
        //            throw new InvalidOperationException("Received response with unexpected TransactionId.");
        //        }


        //        var existingTransaction = transaction;

        //        if (existingTransaction == null)
        //        {
        //            throw new InvalidOperationException("Transaction not found in the database.");
        //        }

        //        var success = await UpdateEntityPropertiesAsync(transaction, entity =>
        //        {


        //            entity.systemConversationId = paymentResponse.message.systemConversationId;
        //            entity.originatorConversationId = paymentResponse.message.originatorConversationId;

        //        });
        //        // return updateResult; // Return the result of the update operation
        //        return success;
        //    }
        //    catch (Exception ex) { }

        //    return false;


        //}

     

    }
}
