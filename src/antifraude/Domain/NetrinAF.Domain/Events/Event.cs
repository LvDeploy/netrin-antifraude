namespace NetrinAF.Domain.Events
{
    public abstract class Event
    {
        public DateTimeOffset TimeStamp { get; set; }

        protected Event()
        {
            TimeStamp = DateTimeOffset.UtcNow;
        }
    }
}
