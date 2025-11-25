namespace Pazaryeri.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Answer { get; set; }
        public string AnsweredDateMessage { get; set; }
        public DateTime CreationDate { get; set; }
        public int CustomerId { get; set; }
        public string ImageUrl { get; set; }
        public string ProductName { get; set; }
        public bool Public { get; set; }=false;
        public bool ShowUserName { get; set; }=false;
        public string Status { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public string WebUrl { get; set; }
        public string ProductMainId { get; set; }
        public Platform Platform { get; set; }

    }
}
