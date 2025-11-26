using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface ITransactionRepository:IRepository<Transaction>
    {
        Task<(List<Transaction> Transactions, int TotalCount)> GetPagedTransactionsAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Transaction> TransactionExistsAsync(string transactionId);
    }
}
