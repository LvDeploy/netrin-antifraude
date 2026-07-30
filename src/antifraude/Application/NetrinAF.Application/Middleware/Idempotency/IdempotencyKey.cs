namespace NetrinAF.Application.Middleware.Idempotency
{
    public class IdempotencyKey()
    {
        private static readonly AsyncLocal<string?> Current = new();

        public string Get()
        {
            return Current.Value
                ?? string.Empty;
        }

        public void Set(string value)
        {
            Current.Value = value;
        }
    }
}
