using System;
using System.Collections.Generic;

namespace MoonGrid
{
    public class QueryResult<T>
    {
        public ICollection<T> ResultData { get; set; } = Array.Empty<T>();
        public bool HasMoreData { get; set; }
        public string Error { get; set; }
        public int PageCount { get; set; }

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

        public QueryResult(string error)
        {
            Error = error;
        }
    }
}
