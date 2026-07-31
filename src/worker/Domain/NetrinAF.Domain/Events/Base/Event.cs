namespace NetrinAF.Domain.Events.Base
{
    public abstract class Event
    {
        public DateTimeOffset TimeStamp { get; set; }
        public string CorrelationId { get; set; }

        protected Event()
        {
            TimeStamp = DateTimeOffset.UtcNow;
        }
    }
}
