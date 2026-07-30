using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NetrinAF.Application.Middleware.Idempotency
{
    public class IdempotencyKeyMiddleware(RequestDelegate next)
    {
        private const string HeaderName = "X-Idempotency-Key";
        private const string MissingHeaderMessage = "The X-Idempotency-Key header is required for POST requests.";

        public async Task InvokeAsync(HttpContext context, IdempotencyKey idempotencyKey)
        {
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues headerValue)
                || StringValues.IsNullOrEmpty(headerValue)
                || string.IsNullOrWhiteSpace(headerValue.ToString()))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { MissingHeaderMessage }
                }, context.RequestAborted);
                return;
            }

            idempotencyKey.Set(headerValue.ToString());
            await next(context);
        }
    }
}
