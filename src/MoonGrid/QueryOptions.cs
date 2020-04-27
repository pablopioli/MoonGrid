namespace MoonGrid
{
    public class QueryOptions
    {
        public string Order { get; set; }
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
    }
}
