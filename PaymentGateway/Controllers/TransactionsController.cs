using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Models;

namespace PaymentGateway.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly PaymentGatewayContext _context;
        private readonly IMapper _mapper;

       

        public TransactionsController(PaymentGatewayContext context, IMapper mapper)
        {
            _context = context;
            this._mapper = mapper;
        }

        // Fetch the Transaction from the database
        [HttpGet]
        public async Task<IActionResult> GetData()
        {
           
            Transaction transaction = await _context.Transaction.FindAsync(1);

            if (transaction == null)
            {
                return NotFound(); // Handle the case where the Transaction is not found.
            }

            // Convert the Transaction to TransactionViewModel using the mapper.
            TransactionViewModel transactionViewModel = _mapper.Map<TransactionViewModel>(transaction);

            return View(transactionViewModel);
        }


        

       

        // GET: Transactions/transaction
        public IActionResult Transaction()
        {
            return View(new TransactionViewModel());
        }

        // POST: Transactions/Transacton
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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


     

    }
}
