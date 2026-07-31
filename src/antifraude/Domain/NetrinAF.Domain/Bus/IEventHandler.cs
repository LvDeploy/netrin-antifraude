using NetrinAF.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetrinAF.Domain.Bus
{
    public interface IEventHandler<in TEvent> where TEvent : Event
    {
        Task Handler(TEvent @event);
    }
}
