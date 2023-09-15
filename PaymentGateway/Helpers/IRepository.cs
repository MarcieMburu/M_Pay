using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;

namespace PaymentGateway.Helpers
{
    public interface IRepository
        <TEntity> where TEntity : class
    {
        Task<string> GetBearerToken(string client_id, string client_secret);
        Task<bool> ProcessPaymentOrderAsync<TEntity>(Transaction transaction, Func<TEntity, ZamupayRequest> mapToZamupayRequest);
         ZamupayRequest MapTransactionToZamupayRequest(Transaction transaction);
        Task<List<Transaction>> GetTransactionsAsync();
        Task<ActionResult<Transaction>> GetLatestTransaction();
        Task<bool> UpdateEntityPropertiesAsync<TEntity>(TEntity entity, Func<TEntity, Task<bool>> updateAction);
   
    }
}
