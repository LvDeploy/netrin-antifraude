namespace NetrinAF.Api.ApiResults
{
    public class ResultFailure
    {
        public ResultFailure(string correlationId, List<string> errors)
        {
            CorrelationId = correlationId;
            Errors = errors;
        }

        public string CorrelationId { get; private set; }
        public List<string> Errors { get; private set; }
    }
}
