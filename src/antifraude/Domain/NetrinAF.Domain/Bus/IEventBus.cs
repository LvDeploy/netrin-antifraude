using NetrinAF.Domain.Events;

namespace NetrinAF.Domain.Bus
{
    public interface IEventBus
    {
        Task SendCommand<T>(T command);

        void Publish<T>(T @event);

        void Subscribe<T, H>() 
            where T : Event
            where H : IEventHandler<T>;
    }
}
