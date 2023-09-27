using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Data;
using PaymentGateway.Models;
using PaymentGateway.Helpers;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using PaymentGateway.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Protocol.Core.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;

namespace PaymentGateway.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly PaymentGatewayContext _context;

        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient = new HttpClient();
        private ApiSettings _apiSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private AccessToken _accessToken;
        public IRepository<Transaction> _repository;
        private readonly IMemoryCache _memoryCache;
        private IDistributedCache _distributedCache;
        private readonly IRequestClient<TransactionViewModel> _requestClient;


        public TransactionsController(PaymentGatewayContext context, IMemoryCache memoryCache,
          IRepository<Transaction> repository,IRequestClient<TransactionViewModel> requestClient,
          IMapper mapper, IOptions<ApiSettings> apiSettings,
            HttpClient httpClient, IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
        {
            _context = context;
            _repository = repository;
            this._mapper = mapper;
            _memoryCache = memoryCache;
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _distributedCache = distributedCache;
            _requestClient = requestClient;
        }



        // GET: Transactions/transaction
        public async Task<IActionResult> Transaction()
        {
            TransactionViewModel transactionViewModel = new TransactionViewModel();
           
            transactionViewModel.originatorConversationId = Guid.NewGuid().ToString();
            transactionViewModel.systemTraceAuditNumber = Guid.NewGuid().ToString();
            transactionViewModel.CreatedBy = "Web App";



            return View(transactionViewModel);
        }
        public async Task<IActionResult> Index()
        {
            

            return View();
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
        public async Task<TransactionRoute> GetTransactionRouteAsync()
        {

            var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
            var transactionRouteEndpoint = $"{_apiSettings.base_api_url}/v1/transaction-routes/assigned-routes";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var routeResponse = await _httpClient.GetAsync(transactionRouteEndpoint);
            routeResponse.EnsureSuccessStatusCode();
            var routeResponseContent = await routeResponse.Content.ReadAsStringAsync();
            var transactionRoute = JsonConvert.DeserializeObject<TransactionRoute>(routeResponseContent);

            return transactionRoute;
        }

        
        public async Task<JsonResult> GetTransactionRoute()
        {
            try
            {
                var transactionRoute = await GetTransactionRouteAsync();

                List<SelectListItem> routeItems = new List<SelectListItem>();

                foreach (var route in transactionRoute.routes)
                {
                    routeItems.Add(new SelectListItem
                    {
                        Value = route.id.ToString(),
                        Text = route.categoryDescription
                    });
                }

                return Json(routeItems);
            }
            catch (Exception ex)
            {
                // Handle exception if needed
                return null;
            }
        }

        public async Task<JsonResult> GetChannelTypeByRouteId(string id)
        {
            try
            {
                var transactionRoute = await GetTransactionRouteAsync();

                var channelTypeItems = new List<ChannelTypeItemViewModel>();

                foreach (var route in transactionRoute.routes.Where(x => x.id == id))
                {
                    foreach (var channelTypes in route.channelTypes)
                    {
                        channelTypeItems.Add(new ChannelTypeItemViewModel
                        {
                            RouteId = route.id.ToString(),
                            Value = channelTypes.channelType.ToString(),
                            Text = channelTypes.channelDescription
                        });
                    }
                }

                return Json(channelTypeItems);
            }
            catch (Exception ex)
            {
                // Handle exception if needed
                return null;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transaction(TransactionViewModel transactionViewModel)
        {
            
            transactionViewModel.Date = DateTime.Now;
          

            Transaction transaction = _mapper.Map<Transaction>(transactionViewModel);
            

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Transaction has been successfully saved to the database.";

                await _requestClient.GetResponse<TransactionViewModel>(transactionViewModel, timeout: TimeSpan.FromSeconds(60));


                return RedirectToAction(nameof(Index));
            }


            return View(transactionViewModel);

        }



        public IActionResult DisplayData()
        {
           
            return View();
        }
        //Post data to jquery datatable
        public JsonResult DisplayDataToDataTable()

        {
             List<Transaction> transactions = new List<Transaction>();
            try
            {
                var cachedTransactions = _distributedCache.GetString("CachedTransactions");
                if (cachedTransactions != null)
                {
                    transactions = JsonConvert.DeserializeObject<List<Transaction>>(cachedTransactions);
                }
                else
                {

                    transactions = _context.Transaction.ToList<Transaction>();
                    _distributedCache.SetString("CachedTransactions", JsonConvert.SerializeObject(transactions));
                }
            }
            catch (Exception ex)
            {

                transactions = _context.Transaction.ToList<Transaction>();
            }
            return Json(transactions);
        }

        //       public async Task<IActionResult> ProcessPaymentOrderAsync([FromBody] Transaction transaction)
        //       {
        //           try
        //           {

        //               var accessToken = await _repository.GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);


        //               bool result = await _repository.ProcessPaymentOrderAsync<Transaction>(
        //    transaction,
        //    entity =>
        //    {

        //        var zamupayRequest = MapTransactionToZamupayRequest(transaction);
        //        return zamupayRequest;
        //    }
        //);


        //               if (result)
        //               {
        //                   return Ok("Payment order processed successfully.");
        //               }
        //               else
        //               {
        //                   return BadRequest("Payment order processing failed.");
        //               }
        //           }
        //           catch (Exception ex)
        //           {
        //               // Handle exceptions here
        //               return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the payment order.");
        //           }
        //       }

        //       public async ZamupayRequest MapTransactionToZamupayRequest(Transaction transaction)
        //       {

        //           await _repository.MapTransactionToZamupayRequest(transaction);
        //               }

        //       [HttpPost]
        //       public async Task<ActionResult> PaymentOrderRequest(int id, string originatorConversationId)
        //       {
        //           var transaction = await _context.Transaction.FindAsync(id);

        //           if (transaction == null)
        //           {
        //               return NotFound();
        //           }

        //           if (transaction.IsPosted)
        //           {
        //               return Ok(new { isTransactionPosted = true });
        //           }
        //           else
        //           {
        //               var isPaymentOrderRequestSuccessful = await _repository.ProcessPaymentOrderAsync<Transaction>(
        //    transaction,
        //    entity =>
        //    {

        //        var zamupayRequest = MapTransactionToZamupayRequest(entity);
        //        return zamupayRequest;
        //    }
        //);


        //               if (isPaymentOrderRequestSuccessful)
        //               {
        //                   var updateResult = await _repository.UpdateEntityPropertiesAsync(
        //                transaction,
        //               async entity =>
        //                {
        //                    entity.IsPosted = true;
        //                    return true;
        //                }
        //            );

        //                   if (updateResult)
        //                   {
        //                       return Ok(new { isPosted = true });
        //                   }
        //                   else
        //                   {
        //                       return Ok(new { Message = "ERROR" });
        //                   }
        //               }
        //               else
        //               {
        //                   return Ok(new { Message = "ERROR" });
        //               }
        //           }
        //       }


        [HttpPost]
        public async Task<ActionResult> PaymentOrderRequest(int id, string originatorConversationId)
        {
            var transaction = await _context.Transaction.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.IsPosted)
            {
                return Ok(new { isTransactionPosted = true });
            }
            else
            {
                var isPaymentOrderRequestSuccessful = await ProcessPaymentOrderAsync(transaction);

                if (isPaymentOrderRequestSuccessful)
                {
                    var updateResult = await _repository.UpdateEntityPropertiesAsync(
                 transaction,
                async entity =>
                {
                    entity.IsPosted = true;
                    return true;
                }
             );

                    if (updateResult)
                    {
                        return Ok(new { isPosted = true });
                    }
                    else
                    {
                        return Ok(new { Message = "ERROR" });
                    }
                }
                else
                {
                    return Ok(new { Message = "ERROR" });
                }
            }
        }
        [HttpGet]//get transaction and sends to api
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

                var updateResult = await _repository.UpdateEntityPropertiesAsync(transaction, async entity =>
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


        public ActionResult TransactionRoute()
        {
            return TransactionRoute();
        }
        public async Task<ActionResult> ViewPaymentDetails(int id)

        {
            var transaction = await _context.Transaction.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }


            var transactionViewModel = _mapper.Map<TransactionViewModel>(transaction);

            return PartialView("_TransactionDetails", transactionViewModel);
        }

        public async Task<ActionResult> GetPaymentDetailsByOriginatorConversationId(string originatorConversationId)
        {
            try
            {
                var accessToken = await _repository.GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

               // var isPaymentDetailsSuccessful = await UpdateResultCodeByStatusId(originatorConversationId);

                var transaction = await _context.Transaction.FirstOrDefaultAsync(t => t.originatorConversationId == originatorConversationId && t.IsPosted);
                if (transaction == null)
                {

                    return Json(new { error = "Transaction is not posted or not found." });
                }


                var apiUrl = $"{_apiSettings.base_api_url}/v1/payment-order/check-status?IdType=OriginatorConversationId&Id={originatorConversationId}";
                var paymentOrderDetailsResponse = await _httpClient.GetAsync(apiUrl);
                //paymentOrderDetailsResponse.EnsureSuccessStatusCode();

               
                var responseContent = await paymentOrderDetailsResponse.Content.ReadAsStringAsync();
                var paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(responseContent);


                return PartialView("_PaymentDetails", paymentDetails);
            }
            catch (Exception ex)
            {

                return Json(new { error = "An error occurred while fetching payment details." });
            }
        }

       
        private async Task<int> QueryTransactionStatusIdFromZamupay(string originatorConversationId)
        {
            var apiUrl = $"{_apiSettings.base_api_url}/v1/payment-order/check-status?IdType=OriginatorConversationId&Id={originatorConversationId}";

            var response = await _httpClient.GetAsync(apiUrl);
            var responseContent = await response.Content.ReadAsStringAsync();
            var transactionOutcome = JsonConvert.DeserializeObject<TransactionOutcome>(responseContent);

            return transactionOutcome.transactionStatus;
        }


        public async Task<JsonResult> UpdateResultCodeByStatusId(string originatorConversationId)
        {
            try
            {
                var accessToken = await _repository.GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var apiUrl = $"{_apiSettings.base_api_url}/v1/payment-order/check-status?IdType=OriginatorConversationId&Id={originatorConversationId}";
                var paymentOrderDetailsResponse = await _httpClient.GetAsync(apiUrl);
                paymentOrderDetailsResponse.EnsureSuccessStatusCode();

                var responseContent = await paymentOrderDetailsResponse.Content.ReadAsStringAsync();
                var paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(responseContent);

            
                if (paymentDetails?.orderLines?.FirstOrDefault()?.transactionOutcome?.transactionStatus != null)
                {
                    var resultCode = paymentDetails.orderLines.First().transactionOutcome.resultCode;
                    var transactionToUpdate = _context.Transaction.FirstOrDefault(t => t.originatorConversationId == originatorConversationId);
                    if (transactionToUpdate != null)
                    {
                        var updateEntity = await _repository.UpdateEntityPropertiesAsync(transactionToUpdate, async entity =>
                        {
                            entity.resultCode = resultCode?.ToString();
                            entity.resultCodeDescription = resultCode?.ToString();

                            return true;
                        });

                        if (updateEntity)
                        {
                            return Json(new { message = "Result code updated successfully." });
                        }
                        else
                        {
                            return Json(new { error = "Failed to update result code." });
                        }
                    }

                    return Json(new { error = "Transaction not found." });
                }

                // Return a response or perform an action for other cases
                return Json(new { message = "Transaction status is not 4." });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch payment details or update result code." });
            }
        }

      
        public class ChannelTypeItemViewModel : SelectListItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
            public string RouteId { get; set; }
            public string categoryDescription { get; set; }
        }

    }
}
