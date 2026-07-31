using NetrinAF.Domain.Events.Base;

namespace NetrinAF.Domain.Bus
{
    public interface IEventBus
    {
        void Subscribe<T, H>() 
            where T : Event
            where H : IEventHandler<T>;
    }
}
