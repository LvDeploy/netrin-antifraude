using Microsoft.AspNetCore.Http;

namespace NetrinAF.Application.Middleware.Correlation
{
    public class CorrelationIdMiddleware(RequestDelegate next)
    {
        private const string HeaderName = "X-Correlation-Id";

        public async Task InvokeAsync(HttpContext context, CorrelationId correlationId)
        {
            correlationId.Set();
            context.Response.Headers[HeaderName] = correlationId.Get();
            await next(context);
        }
    }
}
