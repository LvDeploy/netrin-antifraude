using Microsoft.Data.SqlClient;
using NetrinAF.Api.Endpoints;
using NetrinAF.Application.Abstractions.Response;
using Polly;
using Polly.Fallback;
using Polly.Retry;

namespace NetrinAF.Api.Configurations
{
    internal static class ResilienceConfiguration
    {
        internal static void AddResiliencePipelineConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddResiliencePipeline<string, BaseResponse<Guid>>(ResilienceConstants.BasicCommand,
                pipelineBuilder => 
                {
                    pipelineBuilder.AddFallback(new FallbackStrategyOptions<BaseResponse<Guid>>
                    {
                        FallbackAction = _ => Outcome.FromResultAsValueTask<BaseResponse<Guid>>(new BaseResponse<Guid>() { Data = Guid.Empty })
                    });
                    pipelineBuilder.AddRetry(new RetryStrategyOptions<BaseResponse<Guid>>
                    {
                        MaxRetryAttempts = 2,
                        Delay = TimeSpan.FromSeconds(2),
                        BackoffType = DelayBackoffType.Exponential,
                        ShouldHandle = new PredicateBuilder<BaseResponse<Guid>>()
                               .Handle<SqlException>(),
                        OnRetry = retryArguments =>
                        {
                            Console.WriteLine($"Current Attempt {retryArguments.AttemptNumber}, {retryArguments.Outcome}");
                            return ValueTask.CompletedTask;
                        }
                    });
                    pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(20));
                });
        }

    }
}
