namespace Pazaryeri.Entity.Trendyol.Response
{
    public class RequestItem
    {
        public RequestItem requestItem { get; set; }
        public string status { get; set; }
        public List<object> failureReasons { get; set; }
    }
}
