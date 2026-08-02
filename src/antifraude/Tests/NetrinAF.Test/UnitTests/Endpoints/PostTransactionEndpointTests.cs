using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Polly;
using NetrinAF.Api.ApiResults;
using NetrinAF.Api.Endpoints;
using NetrinAF.Api.Endpoints.v1.Transaction;
using NetrinAF.Application.Abstractions.Handler;
using NetrinAF.Application.Abstractions.Response;
using NetrinAF.Application.Commands.CreateTransaction;
using NetrinAF.Application.Middleware.Correlation;
using NetrinAF.Domain.Enums;
using NetrinAF.Domain.ValueObjects;
using NetrinAF.Test.Builders.Commands;
using System.Net;
using System.Net.Http.Json;

namespace NetrinAF.Test.UnitTests.Endpoints;

public sealed class PostTransactionEndpointTests
{
    [Fact]
    public async Task Post_WithSuccessfulHandlerResponse_ReturnsOkResponse()
    {
        Guid transactionId = Guid.NewGuid();
        const string correlationIdValue = "post-correlation-id";
        var command = new CreateTransactionCommandBuilder().WithValue(100m).Build();
        var handler = new Mock<ICommandHandler<CreateTransactionCommand>>();
        handler.Setup(x => x.Handle(It.Is<CreateTransactionCommand>(request => request.Value == 100m), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseBuilder.Success(transactionId));
        await using WebApplication app = await CreateApplication(handler.Object, correlationIdValue);

        var response = await app.GetTestClient().PostAsJsonAsync("/netrin-af/v1/Transactions", command);
        var payload = await response.Content.ReadFromJsonAsync<ResultSuccess<Guid>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(correlationIdValue, payload.CorrelationId);
        Assert.Equal(transactionId, payload.Data);
        handler.Verify(x => x.Handle(It.Is<CreateTransactionCommand>(request => request.Value == 100m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_WithBadRequestHandlerResponse_ReturnsBadRequestResponse()
    {
        const string correlationIdValue = "post-correlation-id";
        var command = new CreateTransactionCommandBuilder().WithValue(0m).Build();
        var handler = new Mock<ICommandHandler<CreateTransactionCommand>>();
        handler.Setup(x => x.Handle(It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseBuilder.Failure<Guid>(
                ErrorData.Set("Value is required"),
                ErrorType.BadRequest));
        await using WebApplication app = await CreateApplication(handler.Object, correlationIdValue);

        var response = await app.GetTestClient().PostAsJsonAsync("/netrin-af/v1/Transactions", command);
        var payload = await response.Content.ReadFromJsonAsync<ResultFailure>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(correlationIdValue, payload.CorrelationId);
        Assert.Contains("Value is required", payload.Errors);
    }

    private static async Task<WebApplication> CreateApplication(
        ICommandHandler<CreateTransactionCommand> handler,
        string correlationIdValue)
    {
        var correlationId = new CorrelationId();
        correlationId.Set(correlationIdValue);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(correlationId);
        builder.Services.AddSingleton(handler);
        builder.Services.AddResiliencePipeline<string, BaseResponse<Guid>>(ResilienceConstants.BasicCommand, static _ => { });
        var app = builder.Build();
        app.Use(async (_, next) =>
        {
            correlationId.Set(correlationIdValue);
            await next();
        });
        new PostTransactionEndpoint().MapEndpoint(app.MapGroup("/netrin-af/v1"));
        await app.StartAsync();
        return app;
    }
}
