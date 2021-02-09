using System;

namespace MoonGrid
{
    public class QueryOptions<TItem>
    {
        public string Order { get; set; }
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
        public Action<QueryResult<TItem>> CallBack { get; set; }

        internal QueryOptions<TItem> CreateCopy()
        {
            return new QueryOptions<TItem>
            {
                Order = Order,
                PageSize = PageSize,
                PageNumber = PageNumber,
                CallBack = CallBack
            };
        }
    }
}
