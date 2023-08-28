using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;
using AutoMapper;

using Microsoft.Extensions.Options;
using PaymentGateway.Helpers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using PaymentGateway.DTOs;
using static API.Controllers.TransactionsController;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly PaymentGatewayContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient = new HttpClient();
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string client_id;
        private readonly string client_secret;
        private AccessToken _accessToken;
        public IRepository<Transaction> _repository;
        private readonly ApiSettings _apiSettings;

        public TransactionsController(PaymentGatewayContext context,IRepository<Transaction> repository,  
            IMapper mapper, IOptions<ApiSettings> apiSettings,
            HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _mapper = mapper;
            _httpClient = httpClient;
           _repository = repository;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;
           
        }

        // GET: api/Transactions2
        [HttpGet]

        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransaction()
        {
            if (_context.Transaction == null)
            {
                return NotFound();
            }
            return await _context.Transaction.ToListAsync();
        }




      
        // POST: api/Transactions2
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]

        public async Task<ActionResult> PostTransaction(TransactionViewModel transactionViewModel)
        {
            transactionViewModel.Date = DateTime.Now;
            transactionViewModel.CreatedBy = "API";

            if (ModelState.IsValid)
            {
                Transaction transaction = _mapper.Map<Transaction>(transactionViewModel);


                if ( transaction.originatorConversationId != null)
                {
                    _context.Transaction.Add(transaction);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);

                    //bool isPaymentOrderSuccessful = await ProcessPaymentOrderAsync(transaction);
                    
                    //if (isPaymentOrderSuccessful)
                    //{
                    //    transaction = await GetTransactionById(transaction.Id);

                    //    return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
                    //}
                    //else
                    //{
                    //    return NoContent();
                    //}

                }
                else { return NoContent(); }
            }
            return BadRequest();
        }

     



        //[HttpPost("api/controller/ProcessPayment")]
        //public async Task<Transaction> GetTransactionById(int id)
        //{
        //    var transaction = await _context.Transaction.FindAsync(id);

        //    if (transaction == null)
        //    {
        //        return null;
        //    }

        //    return transaction;      
        //}



        //[HttpGet("api/controller/ProcessPaymentOrder")]
        //public async Task<bool> ProcessPaymentOrderAsync(Transaction transaction)
        //{
        //    var _httpClient = _httpClientFactory.CreateClient();
        //    var accessToken = await _repository.GetBearerToken(client_id, client_secret);
        //    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //    var paymentOrderRequest = "https://sandboxapi.zamupay.com/v1/payment-order/new-order";

        //    try
        //    {
        //        ZamupayRequest orderRequest = new ZamupayRequest();

        //        RecipientItem recipientItem = new RecipientItem();
        //        RemitterItem remitterItem = new RemitterItem();
        //        TransactionItem transactionItem = new TransactionItem();

        //        remitterItem.name = transaction.Sender.Name;
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
        //             {
        //                paymentOrderLines
        //            };

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

        //        var updateResult = await _repository.UpdateEntityPropertiesAsync(transaction, async entity =>
        //        {


        //            entity.systemConversationId = paymentResponse.message.systemConversationId;
        //            entity.originatorConversationId = paymentResponse.message.originatorConversationId;
        //            entity.IsPosted = true;
        //            return true;
        //        });

        //        if (updateResult)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        return false;
        //    }
        //}



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
