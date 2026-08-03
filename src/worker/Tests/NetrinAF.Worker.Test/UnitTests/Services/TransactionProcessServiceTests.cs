using Microsoft.Extensions.Logging;
using Moq;
using NetrinAF.Application.Services.TransactionProcess;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Contracts.UnitOfWork;
using NetrinAF.Domain.Entities;
using NetrinAF.Domain.Enums;
using NetrinAF.Worker.Test.Builders.Entities;
using NetrinAF.Worker.Test.Builders.Events;

namespace NetrinAF.Worker.Test.UnitTests.Services;

public sealed class TransactionProcessServiceTests
{
    [Fact]
    public async Task Process_WithReviewTransaction_ApprovesTargetRemovesDuplicatesAndCommits()
    {
        const string idempotencyKey = "idempotency-key";
        var target = new TransactionBuilder()
            .WithValue(150m)
            .WithIdempotencyKey(idempotencyKey)
            .Build();
        var duplicate = new TransactionBuilder()
            .WithValue(50m)
            .WithIdempotencyKey(idempotencyKey)
            .WithTrackLog("Duplicate transaction requested")
            .Build();
        TransactionHistoric duplicateHistoric = Assert.Single(duplicate.TrackLog);
        var @event = new TransactionCreatedEventBuilder()
            .WithTransactionId(target.Id)
            .WithIdempotencyKey(idempotencyKey)
            .Build();
        var transactionRepository = new Mock<ITransactionRepository>();
        var historicRepository = new Mock<ITransactionHistoricRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        transactionRepository.Setup(x => x.GetByIdEmpotency(idempotencyKey))
            .ReturnsAsync(new Transaction?[] { target, duplicate });
        unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = CreateService(transactionRepository, historicRepository, unitOfWork);

        await service.Process(@event.TransactionId, @event.IdempotencyKey, @event.CorrelationId);

        Assert.Equal(TransactionStatus.APPROVED, target.Status);
        transactionRepository.Verify(x => x.Update(target), Times.Once);
        transactionRepository.Verify(x => x.Delete(duplicate), Times.Once);
        historicRepository.Verify(x => x.Create(It.IsAny<TransactionHistoric>()), Times.Once);
        historicRepository.Verify(x => x.Delete(duplicateHistoric), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Process_WithMissingTransaction_DoesNotUpdateOrCommit()
    {
        var @event = new TransactionCreatedEventBuilder().Build();
        var transactionRepository = new Mock<ITransactionRepository>();
        var historicRepository = new Mock<ITransactionHistoricRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        transactionRepository.Setup(x => x.GetByIdEmpotency(@event.IdempotencyKey))
            .ReturnsAsync((IEnumerable<Transaction?>?)null!);
        var service = CreateService(transactionRepository, historicRepository, unitOfWork);

        await service.Process(@event.TransactionId, @event.IdempotencyKey, @event.CorrelationId);

        transactionRepository.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Never);
        historicRepository.Verify(x => x.Create(It.IsAny<TransactionHistoric>()), Times.Never);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Process_WithPreviouslyProcessedTransaction_DoesNotUpdateOrCommit()
    {
        const string idempotencyKey = "idempotency-key";
        var target = new TransactionBuilder()
            .WithValue(150m)
            .WithIdempotencyKey(idempotencyKey)
            .WithStatus(TransactionStatus.APPROVED)
            .Build();
        var @event = new TransactionCreatedEventBuilder()
            .WithTransactionId(target.Id)
            .WithIdempotencyKey(idempotencyKey)
            .Build();
        var transactionRepository = new Mock<ITransactionRepository>();
        var historicRepository = new Mock<ITransactionHistoricRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        transactionRepository.Setup(x => x.GetByIdEmpotency(idempotencyKey))
            .ReturnsAsync(new Transaction?[] { target });
        var service = CreateService(transactionRepository, historicRepository, unitOfWork);

        await service.Process(@event.TransactionId, @event.IdempotencyKey, @event.CorrelationId);

        Assert.Equal(TransactionStatus.APPROVED, target.Status);
        transactionRepository.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Never);
        historicRepository.Verify(x => x.Create(It.IsAny<TransactionHistoric>()), Times.Never);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static TransactionProcessService CreateService(
        Mock<ITransactionRepository> transactionRepository,
        Mock<ITransactionHistoricRepository> historicRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new TransactionProcessService(
            historicRepository.Object,
            transactionRepository.Object,
            Mock.Of<ILogger<TransactionProcessService>>(),
            unitOfWork.Object);
    }
}
