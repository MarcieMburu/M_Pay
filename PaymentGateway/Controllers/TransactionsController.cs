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

namespace PaymentGateway.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly PaymentGatewayContext _context;
        private readonly TransactionsRepository _transactionsRepository;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient = new HttpClient();
        private ApiSettings _apiSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private AccessToken _accessToken;
        
        private readonly IMemoryCache _memoryCache;

        public TransactionsController(PaymentGatewayContext context, IMemoryCache memoryCache, TransactionsRepository transactionsRepository, 
            IMapper mapper, IOptions<ApiSettings> apiSettings, HttpClient httpClient,IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _transactionsRepository = transactionsRepository;
            this._mapper = mapper;
            _memoryCache = memoryCache;
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;

        }



        // GET: Transactions/transaction
        public async Task<IActionResult> Transaction()
        {
            return (View(new TransactionViewModel()));
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
        private async Task<TransactionRoute> GetTransactionRouteAsync()
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

            if (ModelState.IsValid)
            {
                Transaction transaction = _mapper.Map<Transaction>(transactionViewModel);

                transaction.Date = DateTime.Now;

                transaction.originatorConversationId = Guid.NewGuid().ToString();
                transaction.systemTraceAuditNumber = Guid.NewGuid().ToString();

                _context.Add(transaction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Transaction has been successfully saved to the database.";


                return RedirectToAction(nameof(DisplayData));
            }

            return View(transactionViewModel);

        }



        public IActionResult DisplayData()
        {
            var successMessage = TempData["SuccessMessage"] as string;

            ViewBag.SuccessMessage = successMessage;
            return View();
        }
        //Post data to jquery datatable
        public JsonResult DisplayDataToDataTable()
        
        {
            List<Transaction> transactions = new List<Transaction>();

            transactions = _context.Transaction.ToList<Transaction>();

            return Json(transactions);
        }


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
                    var updateResult = await _transactionsRepository.UpdateEntityPropertiesAsync(
                 transaction,
                 entity =>
                 {
                     entity.IsPosted = true;
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

                var updateResult = await _transactionsRepository.UpdateEntityPropertiesAsync(
                          existingTransaction,
                          entity =>
                          {

                              entity.systemConversationId = paymentResponse.message.systemConversationId;
                              entity.originatorConversationId = paymentResponse.message.originatorConversationId;

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
                var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var apiUrl = $"{_apiSettings.base_api_url}/v1/payment-order/check-status?IdType=OriginatorConversationId&Id={originatorConversationId}";
                var paymentOrderDetailsResponse = await _httpClient.GetAsync(apiUrl);
                //paymentOrderDetailsResponse.EnsureSuccessStatusCode();

                var responseContent = await paymentOrderDetailsResponse.Content.ReadAsStringAsync();
                var paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(responseContent);


                return PartialView("_PaymentDetails", paymentDetails);
            }
            catch (Exception ex)
            {

                return Json(new { error = "Failed to fetch payment details." });
            }
        }
        public async Task<IActionResult> UpdateResultCodeByStatusId(string originatorConversationId)
        {
            try
            {
                var accessToken = await GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var apiUrl = $"{_apiSettings.base_api_url}/v1/payment-order/check-status?IdType=OriginatorConversationId&Id={originatorConversationId}";
                var paymentOrderDetailsResponse = await _httpClient.GetAsync(apiUrl);
                paymentOrderDetailsResponse.EnsureSuccessStatusCode();

                var responseContent = await paymentOrderDetailsResponse.Content.ReadAsStringAsync();
                var paymentDetails = JsonConvert.DeserializeObject<PaymentDetails>(responseContent);

                // Check if transaction status ID is 4
                if (paymentDetails?.orderLines?.FirstOrDefault()?.transactionOutcome?.transactionStatus == 4)
                {
                    var resultCode = paymentDetails.orderLines.First().transactionOutcome.resultCode;
                    var transactionToUpdate = _context.Transaction.FirstOrDefault(t => t.originatorConversationId == originatorConversationId);
                    if (transactionToUpdate != null)
                    {
                        var success = await _transactionsRepository.UpdateEntityPropertiesAsync(transactionToUpdate, entity =>
                        {
                            entity.resultCode = resultCode?.ToString();
                            entity.resultCodeDescription = resultCode?.ToString();

                        });

                        if (success)
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


        public class PaymentOrderLine
        {
            public RecipientItem recipient { get; set; }
            public TransactionItem transaction { get; set; }

            public RemitterItem remitter { get; set; }
        }

        public class RecipientItem
        {
            public string name { get; set; }
            public string address { get; set; }
            public string emailAddress { get; set; }
            public string phoneNumber { get; set; }
            public string idType { get; set; }
            public string idNumber { get; set; }
            public string financialInstitution { get; set; }
            public string primaryAccountNumber { get; set; }
            public string mccmnc { get; set; }
            public int ccy { get; set; }
            public string country { get; set; }
            public string purpose { get; set; }
        }

        public class RemitterItem
        {
            public string name { get; set; }
            public string address { get; set; }
            public string phoneNumber { get; set; }
            public string idType { get; set; }
            public string idIssuePlace { get; set; }
            public string idNumber { get; set; }
            public string idIssueDate { get; set; }
            public string idExpireDate { get; set; }
            public string nationality { get; set; }
            public string country { get; set; }
            public int ccy { get; set; }
            public string financialInstitution { get; set; }
            public string sourceOfFunds { get; set; }
            public string principalActivity { get; set; }
            public string dateOfBirth { get; set; }
            public string state { get; set; }
            public string city { get; set; }
            public string postalCode { get; set; }
        }


        public class ZamupayRequest
        {
            public string originatorConversationId { get; set; }
            public string paymentNotes { get; set; }
            public List<PaymentOrderLine> paymentOrderLines { get; set; }
        }

        public class TransactionItem
        {
            public string routeId { get; set; }
            public int ChannelType { get; set; }
            public int amount { get; set; }
            public string reference { get; set; }
            public string systemTraceAuditNumber { get; set; }
        }


    }
    public class ApiSettings
    {
        public string client_id { get; set; }

        public string client_secret { get; set; }
        public string grant_type { get; set; }

        public string scope { get; set; }

        public string base_api_url { get; set; }
        public string base_token_url { get; set; }

    }
}
