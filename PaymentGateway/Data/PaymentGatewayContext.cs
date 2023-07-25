using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Transaction>(ConfigureProfile);
        }
        private void ConfigureProfile(EntityTypeBuilder<Transaction> builder)
        {

            builder.OwnsOne(p => p.Sender);
            builder.OwnsOne(p => p.Receiver);
        }
     
    }
}
