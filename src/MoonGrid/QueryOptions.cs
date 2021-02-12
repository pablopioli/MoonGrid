using System;

namespace MoonGrid
{
    public class QueryOptions<TItem>
    {
        public string Order { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; } = 1;
        public Action<QueryResult<TItem>> CallBack { get; set; }
    }
}
