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

        [HttpGet]

        public async Task<IActionResult> GetData()
        {
            // Fetch the Transaction from the database
            Transaction transaction = await _context.Transaction.FindAsync(1);

            if (transaction == null)
            {
                return NotFound(); // Handle the case where the Transaction is not found.
            }

            // Convert the Transaction to TransactionViewModel using the mapper.
            TransactionViewModel transactionViewModel = _mapper.Map<TransactionViewModel>(transaction);

            return View(transactionViewModel);
        }


        // GET: Transactions
        public async Task<IActionResult> Index()
        {
              return _context.Transaction != null ? 
                          View(await _context.Transaction.ToListAsync()) :
                          Problem("Entity set 'PaymentGatewayContext.Transaction'  is null.");
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Transaction == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
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
        public JsonResult GetDataForDataTable()
        {
           List<Transaction> transactions = new List<Transaction>();
            
            transactions = _context.Transaction.ToList<Transaction>(); // Retrieve data from the database

            return Json(transactions); // Return data as JSON to the view
        }
        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Transaction == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SenderName,SenderIDNO,SenderPhoneNo,SenderAccount,ReceiverName,ReceiverIDNO,ReceiverPhoneNo,ReceiverAccount,Amount,Date")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transaction == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transaction
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transaction == null)
            {
                return Problem("Entity set 'PaymentGatewayContext.Transaction'  is null.");
            }
            var transaction = await _context.Transaction.FindAsync(id);
            if (transaction != null)
            {
                _context.Transaction.Remove(transaction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
          return (_context.Transaction?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
