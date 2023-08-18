//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using AutoMapper;
//using Microsoft.Extensions.Options;
//using Newtonsoft.Json;
//using PaymentGateway.Controllers;
//using PaymentGateway.DTOs;
//using PaymentGateway.Helpers;
//using Microsoft.AspNetCore.Mvc;
//using PaymentGateway.Models;
//using System.Linq.Dynamic.Core;
//using Microsoft.AspNetCore.Mvc.Rendering;



////using static PaymentGateway.Controllers.TransactionsController;

//namespace Shared
//{
//    public class PaymentService<TEntity> : IPaymentService<TEntity>
//        where TEntity : class
//    {
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly PaymentGateway.Data.PaymentGatewayContext _context;
//        private readonly TransactionsRepository _transactionsRepository;
//        private readonly IMapper _mapper;
//        private readonly HttpClient _httpClient = new HttpClient();
//        private ApiSettings _apiSettings;
//              private AccessToken _accessToken;

//        public PaymentService(IHttpClientFactory httpClientFactory, PaymentGateway.Data.PaymentGatewayContext context, TransactionsRepository transactionsRepository,
//            IMapper mapper, IOptions<ApiSettings> apiSettings, HttpClient httpClient)
//        {
//            _context = context;
//            _transactionsRepository = transactionsRepository;
//            this._mapper = mapper;
           
//            _httpClient = httpClient;
//            _apiSettings = apiSettings.Value;
            
//            _httpClientFactory = httpClientFactory;
//        }

//        public async Task<string> GetBearerTokenAsync(string client_id, string client_secret)
//        {

//            var requestContent = new FormUrlEncodedContent(new[]
//            {
//                new KeyValuePair<string, string>("client_id",_apiSettings.client_id),
//                new KeyValuePair<string, string>("client_secret",_apiSettings.client_secret),
//                 new KeyValuePair<string, string>("grant_type", _apiSettings.grant_type),
//                 new KeyValuePair<string, string>("scope",_apiSettings.scope)
//        });

//            try
//            {
//                var response = await _httpClient.PostAsync($"{_apiSettings.base_token_url}/connect/token", requestContent);
//                //response.EnsureSuccessStatusCode();
//                var responseContent = await response.Content.ReadAsStringAsync();
//                var accessToken = JsonConvert.DeserializeObject<AccessToken>(responseContent);


//                return accessToken.access_token;
//            }
//            catch (Exception ex)
//            {

//            }

//            return null;
//        }

//        public Task<TransactionRoute> GetTransactionRouteAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<bool> ProcessPaymentOrderAsync(TEntity entity, string originatorConversationId, Func<TEntity, Task<bool>> updateAction)
//        {
//            throw new NotImplementedException();
//        }
//        //public async Task<string> GetBearerToken(string client_id, string client_secret)
//        //{

//        //    var requestContent = new FormUrlEncodedContent(new[]
//        //    {
//        //        new KeyValuePair<string, string>("client_id",_apiSettings.client_id),
//        //        new KeyValuePair<string, string>("client_secret",_apiSettings.client_secret),
//        //         new KeyValuePair<string, string>("grant_type", _apiSettings.grant_type),
//        //         new KeyValuePair<string, string>("scope",_apiSettings.scope)
//        //});

//        //    try
//        //    {
//        //        var response = await _httpClient.PostAsync($"{_apiSettings.base_token_url}/connect/token", requestContent);
//        //        //response.EnsureSuccessStatusCode();
//        //        var responseContent = await response.Content.ReadAsStringAsync();
//        //        var accessToken = JsonConvert.DeserializeObject<AccessToken>(responseContent);


//        //        return accessToken.access_token;
//        //    }
//        //    catch (Exception ex)
//        //    {

//        //    }

//        //    return null;
//        //}
//        //private async Task<TransactionRoute> GetTransactionRouteAsync()
//        //{
//        //    var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
//        //    var transactionRouteEndpoint = $"{_apiSettings.base_api_url}/v1/transaction-routes/assigned-routes";
//        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//        //    var routeResponse = await _httpClient.GetAsync(transactionRouteEndpoint);
//        //    routeResponse.EnsureSuccessStatusCode();
//        //    var routeResponseContent = await routeResponse.Content.ReadAsStringAsync();
//        //    var transactionRoute = JsonConvert.DeserializeObject<TransactionRoute>(routeResponseContent);

//        //    return transactionRoute;
//        //}

//        //public async Task<JsonResult> GetTransactionRoute()
//        //{
//        //    try
//        //    {
//        //        var transactionRoute = await GetTransactionRouteAsync();

//        //        List<SelectListItem> routeItems = new List<SelectListItem>();

//        //        foreach (var route in transactionRoute.routes)
//        //        {
//        //            routeItems.Add(new SelectListItem
//        //            {
//        //                Value = route.id.ToString(),
//        //                Text = route.categoryDescription
//        //            });
//        //        }

//        //        return Json(routeItems);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        // Handle exception if needed
//        //        return null;
//        //    }
//        //}

//        //public async Task<JsonResult> GetChannelTypeByRouteId(string id)
//        //{
//        //    try
//        //    {
//        //        var transactionRoute = await GetTransactionRouteAsync();

//        //        var channelTypeItems = new List<ChannelTypeItemViewModel>();

//        //        foreach (var route in transactionRoute.routes.Where(x => x.id == id))
//        //        {
//        //            foreach (var channelTypes in route.channelTypes)
//        //            {
//        //                channelTypeItems.Add(new ChannelTypeItemViewModel
//        //                {
//        //                    RouteId = route.id.ToString(),
//        //                    Value = channelTypes.channelType.ToString(),
//        //                    Text = channelTypes.channelDescription
//        //                });
//        //            }
//        //        }

//        //        return Json(channelTypeItems);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        // Handle exception if needed
//        //        return null;
//        //    }
//        //}


//        //public async Task<TResponse> PaymentOrderRequestAsync<TRequest, TResponse>(string endpoint, TRequest requestPayload)
//        //{
//        //    var transaction = await _context.Transaction.FindAsync(id);

//        //    if (transaction == null)
//        //    {
//        //        return NotFound();
//        //    }

//        //    if (transaction.IsPosted)
//        //    {
//        //        return Ok(new { isTransactionPosted = true });
//        //    }
//        //    else
//        //    {
//        //        var isPaymentOrderRequestSuccessful = await ProcessPaymentOrderAsync(transaction);

//        //        if (isPaymentOrderRequestSuccessful)
//        //        {
//        //            var updateResult = await _transactionsRepository.UpdateEntityPropertiesAsync(
//        //         transaction,
//        //         entity =>
//        //         {
//        //             entity.IsPosted = true;
//        //         }
//        //     );

//        //            if (updateResult)
//        //            {
//        //                return Ok(new { isPosted = true });
//        //            }
//        //            else
//        //            {
//        //                return Ok(new { Message = "ERROR" });
//        //            }
//        //        }
//        //        else
//        //        {
//        //            return Ok(new { Message = "ERROR" });
//        //        }
//        //    }
//        //}

//        //public async Task<bool> ProcessPaymentOrderAsync(Transaction transaction)
//        //{
//        //    var _httpClient = _httpClientFactory.CreateClient();
//        //    var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
//        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
//        //    var paymentOrderRequest = $"{_apiSettings.base_api_url}/v1/payment-order/new-order";

//        //    try
//        //    {
//        //        ZamupayRequest orderRequest = new ZamupayRequest();

//        //        RecipientItem recipientItem = new RecipientItem();
//        //        RemitterItem remitterItem = new RemitterItem();
//        //        TransactionItem transactionItem = new TransactionItem();

//        //        remitterItem.name = transaction.Sender.Name;
//        //        remitterItem.idNumber = transaction.Sender.ID_NO;
//        //        remitterItem.phoneNumber = transaction.Sender.Phone_No;
//        //        remitterItem.address = "nyeri";
//        //        remitterItem.sourceOfFunds = transaction.Sender.Src_Account;
//        //        remitterItem.ccy = 404;
//        //        remitterItem.country = "Kenya";
//        //        remitterItem.nationality = "Kenyan";
//        //        remitterItem.principalActivity = "Business";
//        //        remitterItem.financialInstitution = "Bank";
//        //        remitterItem.dateOfBirth = "01/01/2002";

//        //        recipientItem.mccmnc = "63902";
//        //        recipientItem.ccy = 404;
//        //        recipientItem.country = "KE";
//        //        recipientItem.name = transaction.Receiver.Name;
//        //        recipientItem.idNumber = transaction.Receiver.ID_NO;
//        //        recipientItem.primaryAccountNumber = transaction.Receiver.Phone_No;
//        //        recipientItem.financialInstitution = "Mpesa";
//        //        recipientItem.idType = "National ID";
//        //        recipientItem.purpose = "TEST";

//        //        transactionItem.amount = transaction.Amount;
//        //        transactionItem.ChannelType = transaction.ChannelType;
//        //        transactionItem.routeId = transaction.RouteId;
//        //        transactionItem.reference = transaction.reference;
//        //        transactionItem.systemTraceAuditNumber = transaction.systemTraceAuditNumber;

//        //        PaymentOrderLine paymentOrderLines = new PaymentOrderLine();

//        //        paymentOrderLines.remitter = remitterItem;
//        //        paymentOrderLines.recipient = recipientItem;
//        //        paymentOrderLines.transaction = transactionItem;

//        //        orderRequest.paymentOrderLines = new List<PaymentOrderLine>
//        //         {
//        //            paymentOrderLines
//        //        };

//        //        orderRequest.paymentNotes = "Transactions";

//        //        orderRequest.originatorConversationId = transaction.originatorConversationId;


//        //        var jsonPayload = JsonConvert.SerializeObject(orderRequest);
//        //        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


//        //        var response = await _httpClient.PostAsync(paymentOrderRequest, httpContent);
//        //        //response.EnsureSuccessStatusCode(); 


//        //        var responseContent = await response.Content.ReadAsStringAsync();


//        //        var paymentResponse = JsonConvert.DeserializeObject<PaymentRequestResponse>(responseContent);

//        //        if (paymentResponse.message.originatorConversationId != transaction.originatorConversationId)
//        //        {
//        //            throw new InvalidOperationException("Received response with unexpected TransactionId.");
//        //        }


//        //        var existingTransaction = transaction;

//        //        if (existingTransaction == null)
//        //        {
//        //            throw new InvalidOperationException("Transaction not found in the database.");
//        //        }

//        //        var updateResult = await _transactionsRepository.UpdateEntityPropertiesAsync(
//        //                  existingTransaction,
//        //                  entity =>
//        //                  {

//        //                      entity.systemConversationId = paymentResponse.message.systemConversationId;
//        //                      entity.originatorConversationId = paymentResponse.message.originatorConversationId;

//        //                  });

//        //        if (updateResult)
//        //        {
//        //            return true;
//        //        }
//        //        else
//        //        {
//        //            return false;
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {

//        //        return false;
//        //    }
//        //}

//        //public Task<string> GetBearerTokenAsync(string client_id, string client_secret)
//        //{
//        //    throw new NotImplementedException();
//        //}

//        //Task<TransactionRoute> IPaymentService<TEntity>.GetTransactionRouteAsync()
//        //{
//        //    throw new NotImplementedException();
//        //}

//        //public Task<bool> ProcessPaymentOrderAsync(TEntity entity, Func<TEntity, Task<bool>> updateAction)
//        //{
//        //    throw new NotImplementedException();
//        //}
//    }
//}

