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
                        Delay = TimeSpan.Zero,
                        ShouldHandle = new PredicateBuilder<BaseResponse<Guid>>()
                               .Handle<Exception>(),
                        OnRetry = retryArguments =>
                        {
                            Console.WriteLine($"Current Attempt {retryArguments.AttemptNumber}, {retryArguments.Outcome}");
                            return ValueTask.CompletedTask;
                        }
                    });
                });
        }

    }
}
