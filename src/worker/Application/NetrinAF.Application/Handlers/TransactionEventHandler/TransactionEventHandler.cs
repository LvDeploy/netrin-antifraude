using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetrinAF.Application.Handlers.TransactionEventHandler
{
    public class TransactionEventHandler() : IEventHandler<TransactionEvent>
    {
        public async Task Handler(TransactionEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
