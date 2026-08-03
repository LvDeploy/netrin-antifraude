using Moq;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Commands.CreateTransaction;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Application.Middleware.Idempotency;
using NetrinAF.Domain.Bus;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Domain.Entities;
using NetrinAF.Domain.Enums;
using NetrinAF.Domain.Events;
using NetrinAF.Test.Builders.Commands;

namespace NetrinAF.Test.UnitTests.Commands;

public sealed class CreateTransactionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_PersistsTransactionPublishesEventAndReturnsSuccess()
    {
        const string correlationIdValue = "correlation-id";
        const string idempotencyKeyValue = "idempotency-key";
        CreateTransactionCommand command = new CreateTransactionCommandBuilder().WithValue(150m).Build();
        var bus = new Mock<IEventBus>();
        var transactionRepository = new Mock<ITransactionRepository>();
        var historicRepository = new Mock<ITransactionHistoricRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var correlationId = new CorrelationId();
        var idempotencyKey = new IdempotencyKey();
        correlationId.Set(correlationIdValue);
        idempotencyKey.Set(idempotencyKeyValue);
        unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new CreateTransactionCommandHandler(
            bus.Object,
            idempotencyKey,
            correlationId,
            historicRepository.Object,
            transactionRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);
        transactionRepository.Verify(x => x.Create(It.Is<Transaction>(transaction =>
            transaction.Value == 150m && transaction.IdEmpotency == idempotencyKeyValue)), Times.Once);
        historicRepository.Verify(x => x.Create(It.Is<TransactionHistoric>(historic =>
            historic.StatusMessage == "Transação Solicitada" && historic.IdEmpotency == idempotencyKeyValue)), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        bus.Verify(x => x.Publish(It.Is<TransactionCreatedEvent>(@event =>
            @event.CorrelationId == correlationIdValue &&
            @event.IdempotencyKey == idempotencyKeyValue &&
            @event.TransactionId == result.Data)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ReturnsBadRequestAndDoesNotPersistOrPublish()
    {
        CreateTransactionCommand command = new CreateTransactionCommandBuilder().WithValue(0m).Build();
        var bus = new Mock<IEventBus>();
        var transactionRepository = new Mock<ITransactionRepository>();
        var historicRepository = new Mock<ITransactionHistoricRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateTransactionCommandHandler(
            bus.Object,
            new IdempotencyKey(),
            new CorrelationId(),
            historicRepository.Object,
            transactionRepository.Object,
            unitOfWork.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Contains(result.Errors!, error => error.Detail == "O Campo Value não pode ser vazio");
        transactionRepository.Verify(x => x.Create(It.IsAny<Transaction>()), Times.Never);
        historicRepository.Verify(x => x.Create(It.IsAny<TransactionHistoric>()), Times.Never);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        bus.Verify(x => x.Publish(It.IsAny<TransactionCreatedEvent>()), Times.Never);
    }
}
