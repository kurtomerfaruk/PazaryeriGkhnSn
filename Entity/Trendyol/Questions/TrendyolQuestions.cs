namespace Pazaryeri.Entity.Trendyol.Questions
{
    public class TrendyolQuestions
    {
        public List<QuestionContent> content { get; set; }
        public int page { get; set; }
        public int size { get; set; }
        public int totalElements { get; set; }
        public int totalPages { get; set; }
    }
}
