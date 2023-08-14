using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.DTOs;


namespace PaymentGateway.Helpers
{
    public class TransactionsRepository
    {
        private readonly PaymentGatewayContext _context;
        public TransactionsRepository(PaymentGatewayContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateEntityPropertiesAsync<TEntity>(
           TEntity existingEntity,
           Action<TEntity> updateAction)
         
           where TEntity : class
        {
            try
            {
                if (existingEntity == null)
                {
                    throw new InvalidOperationException("Entity not found in the database.");
                }

                updateAction(existingEntity);

                _context.Update(existingEntity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return false;
            }
        }
    }
}

