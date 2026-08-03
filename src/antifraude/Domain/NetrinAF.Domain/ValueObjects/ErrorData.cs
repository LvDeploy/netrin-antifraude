namespace NetrinAF.Domain.ValueObjects
{
    public sealed class ErrorData
    {
        public string Detail { get; set; } = string.Empty;

        public static ErrorData Set(string? detail)
        {
            return new ErrorData
            {
                Detail = detail ?? string.Empty
            };
        }
    }
}
