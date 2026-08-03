using Moq;
using NetrinAF.Application.Query.GetTransaction;
using NetrinAF.Domain.Contracts.Repositories;
using NetrinAF.Domain.Enums;
using NetrinAF.Test.Builders.Entities;
using NetrinAF.Test.Builders.Queries;

namespace NetrinAF.Test.UnitTests.Queries;

public sealed class GetTransactionQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingTransaction_ReturnsTransactionAndTrackLog()
    {
        Guid transactionId = Guid.NewGuid();
        var query = new GetTransactionQueryBuilder().WithId(transactionId).Build();
        var transaction = new TransactionBuilder()
            .WithValue(225m)
            .WithTrackLog("Transaction requested")
            .Build();
        transaction.Id = transactionId;
        var repository = new Mock<ITransactionRepository>();
        repository.Setup(x => x.GetWithTrackLog(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        var handler = new GetTransactionQueryHandler(repository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(transactionId, result.Data.TransactionId);
        Assert.Equal(225m, result.Data.Value);
        Assert.Single(result.Data.TrackLog);
        Assert.Equal("Transaction requested", result.Data.TrackLog.Single().StatusMessage);
        repository.Verify(x => x.GetWithTrackLog(transactionId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownTransaction_ReturnsNotFound()
    {
        Guid transactionId = Guid.NewGuid();
        var query = new GetTransactionQueryBuilder().WithId(transactionId).Build();
        var repository = new Mock<ITransactionRepository>();
        repository.Setup(x => x.GetWithTrackLog(transactionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NetrinAF.Domain.Entities.Transaction?)null);
        var handler = new GetTransactionQueryHandler(repository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Contains(result.Errors!, error => error.Detail == "Transacao nao encontrada");
        repository.Verify(x => x.GetWithTrackLog(transactionId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ReturnsBadRequestWithoutCallingRepository()
    {
        var query = new GetTransactionQueryBuilder().WithId(Guid.Empty).Build();
        var repository = new Mock<ITransactionRepository>();
        var handler = new GetTransactionQueryHandler(repository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.BadRequest, result.ErrorType);
        Assert.Contains(result.Errors!, error => error.Detail == "O Campo Id nao pode ser vazio");
        repository.Verify(x => x.GetWithTrackLog(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
