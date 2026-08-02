namespace NetrinAF.Infra.Bus
{
    public class RabbitMqSettings
    {
        public string? DefaultHost { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? DlqName { get; set; }
        public string? DlqRoutingKey { get; set; }
        public string? MainExchange { get; set; }
        public string? MainQueueName { get; set; }
        public string? MainRoutingKey { get; set; }
        public int? RetryTtlMilliseconds { get; set; }
        public long? MaxRetryAttempts { get; set; }
    }
}
