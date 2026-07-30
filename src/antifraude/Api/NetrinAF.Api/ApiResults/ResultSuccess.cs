namespace NetrinAF.Api.ApiResults
{
    public class ResultSuccess<T>
    {
        public ResultSuccess(string correlationId, T data) 
        {
            Data = data;
            CorrelationId = correlationId;
        }
        public T Data { get; private set; }
        public string CorrelationId { get; private set; }
    }
}
