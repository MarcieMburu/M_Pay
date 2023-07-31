using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Data;
using PaymentGateway.Models;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using PaymentGateway.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;



using System.Net.Http.Headers;

namespace PaymentGateway.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly PaymentGatewayContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _serverString;
        public string clientId = "rahab";
        public string clientSecret = "rahab";
        public string credentials = "client_credentials";
        private AccessToken _accessToken;

        public TransactionsController(PaymentGatewayContext context, IMapper mapper, HttpClient httpClient)
        {
            _context = context;
            this._mapper = mapper;
            _httpClient = httpClient;
            _serverString = "https://auth.zamupay.com/connect/token";
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
                var response = await _httpClient.PostAsync(_serverString, requestContent);
                response.EnsureSuccessStatusCode();
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
            routeResponse.EnsureSuccessStatusCode();
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
                foreach (var channelType in route.channelTypes)
                {

                    channelTypeItems.Add(new ChannelTypeItemViewModel
                    {
                        Value = channelType.channelType.ToString(),
                        Text = channelType.channelDescription
                    });
                }
            }

           
            return Json(channelTypeItems);
        }

       


        public ActionResult accessToken()
        {
            return accessToken();
        }
        public ActionResult transactionRoute()
        {
            return transactionRoute(); 
        }
    
            // GET: Transactions/transaction
            public async Task<IActionResult> Transaction()
        {
            
            await GetTransactionRoute();

            return (View(new TransactionViewModel()));
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
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DisplayData));
            }
           

            return View(transactionViewModel);
          

        }
        

        public IActionResult DisplayData()
        {
            return View();
        }
        //Post data to jquery datatable
        public JsonResult GetDataForDataTable()
        {
            List<Transaction> transactions = new List<Transaction>();

            transactions = _context.Transaction.ToList<Transaction>(); // Retrieve data from the database

            return Json(transactions); // Return data as JSON to the view
        }
        public IActionResult AddTransaction(Transaction transaction)
        {
            return View();
        }

        // Fetch the Transaction from the database
        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            Transaction transaction = await _context.Transaction.FindAsync(1);
            if (transaction == null)
            {
                return NotFound(); 
            } // Convert the Transaction to TransactionViewModel using the mapper.
            TransactionViewModel transactionViewModel = _mapper.Map<TransactionViewModel>(transaction);
            return View(transactionViewModel);
        }

        [HttpGet]
        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Transaction());
            else
            {
                using (var db = new PaymentGatewayContext())
                {
                    return View(db.Transaction.Where(x => x.Id == id).FirstOrDefault<Transaction>());
                }
            }
        }


        [HttpPost]
        public JsonResult AddOrEdit(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                using (var db = new PaymentGatewayContext())
                {
                    if (!db.Transaction.Any(x => x.Id == transaction.Id))
                    {
                        db.Transaction.Add(transaction);
                        db.SaveChanges();
                        return Json(transaction);
                    }
                }
            }

            return Json(new { success = false, message = "Invalid data or Transaction with the specified Id already exists." });
        }

     
        [HttpPost]
        public JsonResult DeleteTransaction(int id)
        {
            try
            {
                using (var db = new PaymentGatewayContext())
                {
                    Transaction transaction = db.Transaction.Where(x => x.Id == id).FirstOrDefault<Transaction>();
                    if (transaction != null)
                    {
                        db.Transaction.Remove(transaction);
                        db.SaveChanges();
                        return Json(new { success = true, message = "Deleted Successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Transaction not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception and return an error message
                return Json(new { success = false, message = "An error occurred while deleting the Transaction." });
            }
        }

       
        public ActionResult LoadData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();


                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all transaction data    
                var transactionData = (from temptransaction in _context.Transaction
                                    select temptransaction);

                
                // Sorting
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    transactionData = transactionData.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    transactionData = transactionData.Where(m => m.Sender.Name == searchValue);
                }

                //total number of rows count     
                recordsTotal = transactionData.Count();
                //Paging     
                var data = transactionData.Skip(skip).Take(pageSize).ToList();
                //Returning Json Data    
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            }
            catch (Exception)
            {
                throw;
            }

        }
        [HttpGet]
        public ActionResult Edit(int? ID)
        {
            try
            {
                using (var db = new PaymentGatewayContext())
                {
                    var transaction = (from t in db.Transaction
                                       where t.Id == ID
                                       select t).FirstOrDefault();

                    if (transaction != null)
                    {
                        return View(transaction); 
                    }
                    else
                    {
                        return RedirectToAction("Index"); 
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private class ChannelTypeItemViewModel
        {
            public string Value { get; set; }
            public string Text { get; set; }

            public string categoryDescription { get; set; }
        }
    }
}
