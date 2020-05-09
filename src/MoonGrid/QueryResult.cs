using System.Collections.Generic;

namespace MoonGrid
{
    public class QueryResult<T>
    {
        public ICollection<T> ResultData { get; set; }
        public bool HasMoreData { get; set; }

        public QueryResult()
        { }

        public QueryResult(ICollection<T> resultData)
        {
            ResultData = resultData;
        }

        public QueryResult(ICollection<T> resultData, bool hasMoreData)
        {
            ResultData = resultData;
            HasMoreData = hasMoreData;
        }
    }
}
