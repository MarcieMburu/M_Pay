using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using Shared;

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
        private IDistributedCache _distributedCache;
        private readonly IBusControl _busControl;
        private readonly IRequestClient<TransactionViewModel> _requestClient;
        private readonly IPublishEndpoint _publishEndpoint;



        public TransactionsController(PaymentGatewayContext context, IRepository<Transaction> repository,
            IMapper mapper, IOptions<ApiSettings> apiSettings,HttpClient httpClient, IHttpClientFactory httpClientFactory, 
            IDistributedCache distributedCache, IRequestClient<TransactionViewModel> requestClient, IBusControl busControl,
            IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _httpClient = httpClient;
            _repository = repository;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _distributedCache = distributedCache;
            _requestClient = requestClient;
            _busControl = busControl;
            _publishEndpoint = publishEndpoint;
        }


        // GET: api/Transactions2
        [HttpGet]

        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransaction()
        {
            try
            {
                var cachedTransactions = await _distributedCache.GetStringAsync("CachedTransactions");

                if (cachedTransactions != null)
                {
                    var transactionsFromCache = JsonConvert.DeserializeObject<List<Transaction>>(cachedTransactions);
                    return Ok(transactionsFromCache);
                }

                else
                {

                    var transactions = await _context.Transaction.ToListAsync();

                    if (transactions == null || transactions.Count == 0)
                    {
                        return NotFound();
                    }

                    await _distributedCache.SetStringAsync("CachedTransactions", JsonConvert.SerializeObject(transactions));

                    return Ok(transactions);
                }
            }
            catch (Exception ex)
            {

                var transactions = await _context.Transaction.ToListAsync();

                if (transactions == null || transactions.Count == 0)
                {
                    return NotFound();
                }

                return Ok(transactions);
            }

        }





        [HttpPost]

        public async Task<ActionResult> PostTransaction(TransactionViewModel transactionViewModel)
        {
            transactionViewModel.Date = DateTime.Now;
            transactionViewModel.CreatedBy = "API";

            if (ModelState.IsValid)
            {
                Transaction transaction = _mapper.Map<Transaction>(transactionViewModel);


                if (transaction.originatorConversationId != null)
                {
                    _context.Transaction.Add(transaction);
                    await _context.SaveChangesAsync();

                    //var requestTimeout = TimeSpan.FromSeconds(60);
                    //var response = await _requestClient.GetResponse<TransactionViewModel>(new ProcessedTransactionViewModel { /* Populate message properties */ }, timeout: requestTimeout);
                    try
                    {


                        var message = new TransactionViewModel
                        {
                            Amount = transactionViewModel.Amount,
                            SenderName = transactionViewModel.SenderName,
                            SenderID_NO = transactionViewModel.SenderID_NO,
                            SenderPhone_No = transactionViewModel.SenderPhone_No,
                            SenderSrc_Account = transactionViewModel.SenderSrc_Account,
                            ReceiverName = transactionViewModel.ReceiverName,
                            ReceiverPhone_No = transactionViewModel.ReceiverPhone_No,
                            ReceiverDst_Account = transactionViewModel.ReceiverDst_Account,
                            ReceiverID_NO = transactionViewModel.ReceiverID_NO,

                            Date = transactionViewModel.Date



                        };
                        await _requestClient.GetResponse<TransactionViewModel>(transactionViewModel, timeout: TimeSpan.FromSeconds(60));

                        //await _publishEndpoint.Publish(message);
                        Console.WriteLine("Message published successfully.");


                        return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
                    }
                    catch (Exception ex)
                    {
                    Console.WriteLine("Error publishing message to RabbitMQ.");
                    }

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

        //[HttpPost("CancelTransaction")]
        //public async Task<IActionResult> CancelTransaction(string originatorConversationId)
        //{
        //    var _httpClient = _httpClientFactory.CreateClient();
        //      var accessToken = await _repository.GetBearerToken(_apiSettings.client_id, _apiSettings.client_secret);
        //       _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //    var cancelPaymentOrderRequest = "https://sandboxapi.zamupay.com/v1/payment-order/reject-order?OriginatorConversationId={originatorConversationId}";
        //    try
        //    {
                
        //        var paymentOrderlines = new List<PaymentOrderline>
        //        {
        //           new PaymentOrderline
        //    {
        //        originatorConversationId = originatorConversationId
        //    }
        //        };

        //        var cancellationRequest = new TransactionCancellationRequest
        //        {
        //            paymentOrderlines = paymentOrderlines
        //        };

               
        //        var jsonPayload = JsonConvert.SerializeObject(cancellationRequest);

        //        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //        var response = await _httpClient.PostAsync(cancelPaymentOrderRequest, content);

        //        var responseContent = await response.Content.ReadAsStringAsync();

        //        if (response.IsSuccessStatusCode)
        //        {
                    
        //            var transaction = await _context.Transaction.FirstOrDefaultAsync(t => t.originatorConversationId == originatorConversationId);

        //            if (transaction != null)
        //            {
        //                //transaction.IsCancelled = true;
        //                await _context.SaveChangesAsync();
        //            }

        //            return Ok(new { Message = "Transaction cancelled successfully." });
        //        }
        //        else
        //        {
        //            return BadRequest(new { Message = "Failed to cancel the transaction." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing the request." });
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

