﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Data;
using PaymentGateway.Models;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using PaymentGateway.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using Nest;
using System;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace PaymentGateway.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly PaymentGatewayContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string base_token_url;
        private readonly string base_api_url;
        public string clientId = "rahab";
        public string clientSecret = "rahab";
       public string credentials = "client_credentials";
       private readonly IHttpClientFactory _httpClientFactory;
        private AccessToken _accessToken;

        public TransactionsController(PaymentGatewayContext context, IMapper mapper, HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            this._mapper = mapper;
            _httpClient = httpClient;
            base_token_url = "https://auth.zamupay.com";
            base_api_url = "https://sandboxapi.zamupay.com";
            _httpClientFactory = httpClientFactory;
        }



        // GET: Transactions/transaction
        public async Task<IActionResult> Transaction()
        {
            return (View(new TransactionViewModel()));
        }


        public async Task<string> GetBearerToken(string clientId, string clientSecret)
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",clientId),
                new KeyValuePair<string, string>("client_secret",clientSecret),
                 new KeyValuePair<string, string>("grant_type", "client_credentials"),
                 new KeyValuePair<string, string>("scope", "PyPay_api")
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
         public async Task<JsonResult> GetTransactionRoute()
        {
            var accessToken = await GetBearerToken(clientId, clientSecret);
            var transactionRouteEndpoint = "https://sandboxapi.zamupay.com/v1/transaction-routes/assigned-routes";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
                var routeResponse = await _httpClient.GetAsync(transactionRouteEndpoint);
            try
            {
                //routeResponse.EnsureSuccessStatusCode();
                var routeResponseContent = await routeResponse.Content.ReadAsStringAsync();
                var transactionRoute = JsonConvert.DeserializeObject<TransactionRoute>(routeResponseContent);

                List<SelectListItem> routeItems = new List<SelectListItem>();

                var channelTypeItems = new List<ChannelTypeItemViewModel>();

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
            catch(Exception ex){ }

            return null;
        }

        public async Task<JsonResult> GetChannelTypeByRouteId(string id)
        {

            var accessToken = await GetBearerToken(clientId, clientSecret);
            var transactionRouteEndpoint = "https://sandboxapi.zamupay.com/v1/transaction-routes/assigned-routes";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var routeResponse = await _httpClient.GetAsync(transactionRouteEndpoint);
            routeResponse.EnsureSuccessStatusCode();
            var routeResponseContent = await routeResponse.Content.ReadAsStringAsync();
            var transactionRoute = JsonConvert.DeserializeObject<TransactionRoute>(routeResponseContent);

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


        // POST: Transactions/Transaction
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
        public JsonResult GetDataForDataTable()
        {
            List<Transaction> transactions = new List<Transaction>();

            transactions = _context.Transaction.ToList<Transaction>();

            return Json(transactions);
        }


        [HttpPost]
        public async Task<ActionResult> PaymentOrderRequest(int id)
        {

            var transaction = await _context.Transaction.FindAsync(id);

            if (transaction == null)
            {

                return NotFound();
            }
            var isPaymentOrderRequestSuccessful = await ProcessPaymentOrderAsync(transaction);

            if (isPaymentOrderRequestSuccessful)
            {


                return Content("Payment successful!");
            }
            else
            {

                return Ok(new { Message = "ERRor" });
            }
        }



        [HttpGet]//get transaction and sends to api
        public async Task<bool> ProcessPaymentOrderAsync(Transaction transaction)
        {
            var _httpClient = _httpClientFactory.CreateClient();
            var accessToken = await GetBearerToken(clientId, clientSecret);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var paymentOrderRequest = "https://sandboxapi.zamupay.com/v1/payment-order/new-order";

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

               // var existingTransaction = await _context.Transaction.FindAsync(transaction.Id);
               var existingTransaction = transaction;

                if (existingTransaction == null)
                {
                    throw new InvalidOperationException("Transaction not found in the database.");
                }
                existingTransaction.systemConversationId = paymentResponse.message.systemConversationId;
                existingTransaction.originatorConversationId = paymentResponse.message.originatorConversationId;

               // transaction.systemConversationId = paymentResponse.message.systemConversationId;
                //        transaction.originatorConversationId = paymentResponse.message.originatorConversationId;
                        _context.Update(transaction);
                        await _context.SaveChangesAsync();
                    

            return true;
        }
            catch (Exception ex)
            {
               
                return false; 
            }
        }


        // fetch the payment order response from the API and updating the corresponding transaction in the database.

        //public async Task<bool> ProcessPaymentOrderResponseAsync(int id)
        //{
        //    try
        //    {
        //        var accessToken = await GetBearerToken(clientId, clientSecret);
        //        var _httpClient = _httpClientFactory.CreateClient();
        //        var paymentOrderResponseEndpoint = $"https://sandboxapi.zamupay.com/v1/payment-order/{id}/response";

        //        // Set authorization header with access token
        //        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //        // Send a GET request to the payment order response endpoint
        //        var response = await _httpClient.GetAsync(paymentOrderResponseEndpoint);
                
        //        response.EnsureSuccessStatusCode(); // Ensure successful response, else it will throw an exception

        //        // Read the response content
        //        var responseContent = await response.Content.ReadAsStringAsync();

        //        // Deserialize the response to get the paymentResponse object
        //        var paymentResponse = JsonConvert.DeserializeObject<PaymentRequestResponse>(responseContent);

        //        // Update the database with the received conversation IDs
        //        var transaction = await _context.Transaction.FindAsync(id);
        //        if (transaction == null)
        //        {
        //            return false; 
        //        }

        //        transaction.systemConversationId = paymentResponse.message.systemConversationId;
        //        transaction.originatorConversationId = paymentResponse.message.originatorConversationId;

        //        // Save the changes to the database
        //        await _context.SaveChangesAsync();

        //        // Return true to indicate that the update was successful
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any exceptions that might occur during the process
        //        // Log the exception or take appropriate action based on your application's requirements
        //        return false; // Return false to indicate that the update failed
        //    }
        //}


        //public IActionResult PaymentSuccess()
        //{
        //    return View();
        //}
        //public IActionResult PaymentFail() { return View(); }
        //public ActionResult AccessToken()
        //{
        //    return AccessToken () ;
        //}
        public ActionResult TransactionRoute()
        {
            return TransactionRoute(); 
        }
    public async Task<ActionResult> FindPaymentOrderByOriginatorConversationId(string originatorConversationId)
        {
            var accessToken = await GetBearerToken(clientId, clientSecret);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var findPaymentOrderRequest = "{{base_api_url}}/v1/payment-order/check-status?Id=EDK9CFCCQV9B6ACCTGFK&IdType=OriginatorConversationId";
            var response = await httpClient.GetAsync(findPaymentOrderRequest);
            response.EnsureSuccessStatusCode();
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
}
