using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Data;
using PaymentGateway.Models;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using PaymentGateway.DTOs;

using System.Net.Http.Headers;
using System.Text;

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
        /*       public async Task<IActionResult> PostToken()
        {
            try
            {
              
                var clientId = "rahab";
                var clientSecret = "rahab";

                AccessToken accessToken = await GetBearerToken(clientId, clientSecret, _serverString);



                if (accessToken != null)
                {
                  
                    string tokenValue = accessToken.access_token;
                    int expiresIn = accessToken.expires_in;

                   
                }
                else
                {
                    
                    return BadRequest("Failed to get access token from the API.");
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

          
            return View();
        }

       */

        public async Task<string> GetBearerToken(string clientId, string clientSecret)
        {
           
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id",clientId),
                new KeyValuePair<string, string>("client_secret",clientSecret),
                 new KeyValuePair<string, string>("grant_type", "client_credentials"),
                 new KeyValuePair<string, string>("scope", "PyPay_api")
        });


             //var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.PostAsync(_serverString, requestContent);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var accessToken = JsonConvert.DeserializeObject<AccessToken>(responseContent);
                return accessToken.access_token;
            }
            catch(Exception ex)
            {

            }

            return null;
           
           
        
        }

        public async Task<TransactionRoute> GetTransactionRoute()
        {
            var accessToken = await GetBearerToken(clientId, clientSecret);

           
            var transactionRouteEndpoint = "https://sandboxapi.zamupay.com/v1/transaction-routes/assigned-routes";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var routeResponse = await _httpClient.GetAsync(transactionRouteEndpoint);
            routeResponse.EnsureSuccessStatusCode();
            var routeResponseContent = await routeResponse.Content.ReadAsStringAsync();
            var transactionRoute = JsonConvert.DeserializeObject<TransactionRoute>(routeResponseContent);
            return transactionRoute;
        }
        





        /*
        public async Task<TransactionViewModel> GetTransactionRoute()
        {
            try
            {
                var clientId = "rahab";
                var clientSecret = "rahab";

                // Call the GetBearerToken method to get the access token
                var accessToken = await GetBearerToken(clientId, clientSecret, _serverString);

                // Store the access token in the _accessToken variable
                _accessToken = new AccessToken { access_token = accessToken };

                // Now, use the access token to call the API endpoint to get the transaction route
                var availableRoutes = await GetAvailableTransactionRoutesFromApi();

                var transactionViewModel = new TransactionViewModel
                {
                    // Initialize other properties of the TransactionViewModel as needed
                    AvailableRoutes = new SelectList(availableRoutes)
                };

                // Return the TransactionViewModel to the view
                return transactionViewModel;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the API call or other errors
                // For example, handle network errors, timeouts, etc.
                return null; // Or handle the error appropriately
            }
        }
        private async Task<string> GetTransactionRouteFromApi()
        {
            // Make sure the access token is available
            if (_accessToken == null || string.IsNullOrEmpty(_accessToken.access_token))
            {
                throw new InvalidOperationException("Access token is not available.");
            }

            try
            {
                // Add the access token to the Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken.access_token);

                // Make the authenticated GET request to the API's transaction routes endpoint
                var response = await _httpClient.GetAsync("https://sandboxapi.zamupay.com/v1/transaction-routes/assigned-routes");

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content (transaction route) and return it
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the API call
                // For example, handle network errors, timeouts, etc.
                throw ex;
            }
        }

        private async Task<List<string>> GetAvailableTransactionRoutesFromApi()
        {
            // Make sure the access token is available
            if (_accessToken == null || string.IsNullOrEmpty(_accessToken.access_token))
            {
                throw new InvalidOperationException("Access token is not available.");
            }

            try
            {
                // Add the access token to the Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken.access_token);

                // Make the authenticated GET request to the API's transaction routes endpoint
                var response = await _httpClient.GetAsync("https://sandboxapi.zamupay.com/v1/transaction-routes/assigned-routes");

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content (transaction routes) and return it
                var responseContent = await response.Content.ReadAsStringAsync();
                var routes = JsonConvert.DeserializeObject<List<string>>(responseContent);
                return routes;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the API call
                // For example, handle network errors, timeouts, etc.
                throw ex;
            }
        }
        */






        public ActionResult accessToken()
        {
            return accessToken();
        }
        public ActionResult tranactionRoute()
        {
            return tranactionRoute(); 
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
          // transactionViewModel.TransactionRoutes = await GetTransactionRoutesAsync();

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
                return NotFound(); // Handle the case where the Transaction is not found.
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
      
           

    }
}
