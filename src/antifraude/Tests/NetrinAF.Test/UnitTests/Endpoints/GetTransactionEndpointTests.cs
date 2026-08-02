using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetrinAF.Api.ApiResults;
using NetrinAF.Api.Endpoints.v1.Transaction;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Application.Query.GetTransaction;
using NetrinAF.Domain.Enums;
using NetrinAF.Domain.ValueObjects;
using NetrinAF.Test.Builders.Queries;
using System.Net;
using System.Net.Http.Json;

namespace NetrinAF.Test.UnitTests.Endpoints;

public sealed class GetTransactionEndpointTests
{
    [Fact]
    public async Task Get_WithSuccessfulHandlerResponse_ReturnsOkResponse()
    {
        Guid transactionId = Guid.NewGuid();
        const string correlationIdValue = "get-correlation-id";
        var handler = new Mock<IQueryHandler<GetTransactionQuery, GetTransactionQueryResponse>>();
        handler.Setup(x => x.Handle(It.Is<GetTransactionQuery>(query => query.Id == transactionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseBuilder.Success(new GetTransactionQueryResponse(
                transactionId,
                TransactionStatus.REVIEW,
                100m,
                DateTime.UtcNow,
                [])));
        await using WebApplication app = await CreateApplication(handler.Object, correlationIdValue);

        var response = await app.GetTestClient().GetAsync($"/netrin-af/v1/Transactions/{transactionId}");
        var payload = await response.Content.ReadFromJsonAsync<ResultSuccess<GetTransactionQueryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(correlationIdValue, payload.CorrelationId);
        Assert.Equal(transactionId, payload.Data.TransactionId);
        handler.Verify(x => x.Handle(It.Is<GetTransactionQuery>(query => query.Id == transactionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithNotFoundHandlerResponse_ReturnsNotFoundResponse()
    {
        Guid transactionId = Guid.NewGuid();
        const string correlationIdValue = "get-correlation-id";
        var handler = new Mock<IQueryHandler<GetTransactionQuery, GetTransactionQueryResponse>>();
        handler.Setup(x => x.Handle(It.IsAny<GetTransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseBuilder.Failure<GetTransactionQueryResponse>(
                ErrorData.Set("Transaction not found"),
                ErrorType.NotFound));
        await using WebApplication app = await CreateApplication(handler.Object, correlationIdValue);

        var response = await app.GetTestClient().GetAsync($"/netrin-af/v1/Transactions/{transactionId}");
        var payload = await response.Content.ReadFromJsonAsync<ResultFailure>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(correlationIdValue, payload.CorrelationId);
        Assert.Contains("Transaction not found", payload.Errors);
    }

    private static async Task<WebApplication> CreateApplication(
        IQueryHandler<GetTransactionQuery, GetTransactionQueryResponse> handler,
        string correlationIdValue)
    {
        var correlationId = new CorrelationId();
        correlationId.Set(correlationIdValue);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(correlationId);
        builder.Services.AddSingleton(handler);
        var app = builder.Build();
        app.Use(async (_, next) =>
        {
            correlationId.Set(correlationIdValue);
            await next();
        });
        new GetTransactionEndpoint().MapEndpoint(app.MapGroup("/netrin-af/v1"));
        await app.StartAsync();
        return app;
    }
}
