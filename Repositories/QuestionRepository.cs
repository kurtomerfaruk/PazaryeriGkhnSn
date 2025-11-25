using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Pazaryeri.Data;
using Pazaryeri.Models;
using Pazaryeri.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Pazaryeri.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {

        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Question> CreateAsync(Question entity)
        {
            _context.Questions.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var question = await GetByIdAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Question>> GetAllAsync()
        {
            return await _context.Questions.ToListAsync();
        }

        public async Task<Question> GetByIdAsync(int id)
        {
            return await _context.Questions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(List<Question> Questions, int TotalCount)> GetPagedQuestionsAsync(int start, int length, string search, string sortColumn, string sortDirection)
        {
            var query = _context.Questions.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.Text.Contains(search) ||
                    o.Status.Contains(search) ||
                    o.Platform.ToString().Contains(search));
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(GetSortProperty(sortColumn))
                    : query.OrderBy(GetSortProperty(sortColumn));
            }
            else
            {
                query = query.OrderByDescending(o => o.Id);
            }

            var questions = await query
                .Skip(start)
                .Take(length)
                .ToListAsync();

            return (questions, totalCount);
        }

        private static Expression<Func<Models.Question, object>> GetSortProperty(string sortColumn)
        {
            return sortColumn.ToLower() switch
            {
                "id" => question => question.Id,
                "platform" => question => question.Platform,
                "text" => question => question.Text,
                "status" => question => question.Status,
                "creationDate" => question => question.CreationDate,
                _ => brand => brand.Id
            };
        }

        public async Task<Question> UpdateAsync(Question entity)
        {
            _context.Questions.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Question> GetByQuestionIdAsync(int id)
        {
            return await _context.Questions.FirstOrDefaultAsync(x => x.QuestionId == id);
        }
    }
}
