using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.DTOs;


namespace Shared
{    
    public class Repository<TEntity> : IRepository
        where TEntity : class
    {
        PaymentGatewayContext _context;
        public Repository(PaymentGatewayContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateEntityPropertiesAsync<TEntity>(
           TEntity existingEntity,
           Action<TEntity> updateAction)
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

