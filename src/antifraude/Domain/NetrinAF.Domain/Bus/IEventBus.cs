using NetrinAF.Domain.Events.Base;

namespace NetrinAF.Domain.Bus
{
    public interface IEventBus
    {
        void Publish<T>(T @event);
    }
}
