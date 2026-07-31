using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Application.Middleware.Idempotency;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Domain.Entities;
using NetrinAF.Domain.Enums;
using NetrinAF.Domain.Events;
using NetrinAF.Domain.ValueObjects;

namespace NetrinAF.Application.Commands.CreateTransaction
{
    public sealed class CreateTransactionCommandHandler(IEventBus bus, 
        IdempotencyKey idempotencyKey, 
        CorrelationId correlationId, 
        ITransactionHistoricRepository transactionHistoricRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand>
    {
        public async Task<BaseResponse<Guid>> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            if (!command.IsValid())
            {
                return ResponseBuilder.Failure<Guid>(command.ValidationResult.Errors.Select(x => ErrorData.Set(x.ErrorMessage)), ErrorType.BadRequest);
            }

            var entityTransaction = Transaction.CreateNew(command.Value, idempotencyKey.Get());
            var entityTransactionHistoric = TransactionHistoric.CreateNew(entityTransaction, idempotencyKey.Get(), "Transação Solicitada");

            transactionRepository.Create(entityTransaction);
            transactionHistoricRepository.Create(entityTransactionHistoric);
            await unitOfWork.CommitAsync(cancellationToken);

            bus.Publish(new TransactionEvent(correlationId.Get(), entityTransaction.Id));

            return ResponseBuilder.Success(entityTransaction.Id);
        }
    }
}
