using Pazaryeri.Models;

namespace Pazaryeri.Repositories.Interfaces
{
    public interface IQuestionRepository:IRepository<Question>
    {
        Task<Question> GetByQuestionIdAsync(int id);
    }
}
