using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IQuestionRepository:IRepository<Question>
    {
        Task<(List<Question> Questions, int TotalCount)> GetPagedQuestionsAsync(int start, int length, string search, string sortColumn, string sortDirection);
        Task<Question> GetByQuestionIdAsync(int id);
    }
}
