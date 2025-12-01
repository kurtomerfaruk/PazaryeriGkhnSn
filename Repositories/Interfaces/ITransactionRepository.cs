using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface ITransactionRepository:IRepository<Transaction>
    {
        Task<Transaction> TransactionExistsAsync(string transactionId);
    }
}
