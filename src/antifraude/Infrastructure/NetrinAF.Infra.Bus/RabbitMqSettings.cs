namespace NetrinAF.Infra.Bus
{
    public class RabbitMqSettings
    {
        public string? DefaultHost { get; set; }
        public string? DlqName { get; set; }
        public string? DlqRoutingKey { get; set; }
        public string? MainExchange { get; set; }
    }
}
