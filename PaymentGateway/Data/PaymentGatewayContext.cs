using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Models;

namespace PaymentGateway.Data
{
    public class PaymentGatewayContext : DbContext
    {
        public PaymentGatewayContext (DbContextOptions<PaymentGatewayContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentGateway.Models.Transaction> Transaction { get; set; } = default!;
    }
}
