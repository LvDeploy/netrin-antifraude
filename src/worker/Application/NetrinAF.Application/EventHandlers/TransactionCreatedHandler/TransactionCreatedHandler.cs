using NetrinAF.Application.Services.TransactionProcess;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Events;

namespace NetrinAF.Application.EventHandlers.TransactionCreatedHandler
{
    public class TransactionCreatedHandler(ITransactionProcessService transactionProcessService) : IEventHandler<TransactionCreatedEvent>
    {
        public async Task Handler(TransactionCreatedEvent @event)
        {
            try
            {
                await transactionProcessService.Process(
                         transactionId: @event.TransactionId,
                         idEmpotencykey: @event.IdempotencyKey,
                         correlationId: @event.CorrelationId
                      );
            }
            catch (Exception ex) {
                throw;
            }
        }
    }
}
