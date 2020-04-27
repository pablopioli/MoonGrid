namespace MoonGrid
{
    public class QueryResult<T>
    {
        public T[] ResultData { get; set; }
        public bool HasMoreData { get; set; }

        public QueryResult()
        { }

        public QueryResult(T[] resultData)
        {
            ResultData = resultData;
        }

        public QueryResult(T[] resultData, bool hasMoreData)
        {
            ResultData = resultData;
            HasMoreData = hasMoreData;
        }
    }
}
