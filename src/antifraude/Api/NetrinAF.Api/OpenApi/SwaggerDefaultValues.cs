using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace NetrinAF.Api.OpenApi
{
    [ExcludeFromCodeCoverage]
    public class SwaggerDefaultValues : IOperationFilter
    {
        private const string IdempotencyHeaderName = "X-Idempotency-Key";

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
            foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
            {
                // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
                var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                var response = operation.Responses[responseKey];

                foreach (var contentType in response.Content.Keys)
                {
                    if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }

            if (!string.Equals(apiDescription.HttpMethod, HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            operation.Parameters ??= [];

            bool hasIdempotencyHeader = operation.Parameters.Any(parameter =>
                string.Equals(parameter.Name, IdempotencyHeaderName, StringComparison.OrdinalIgnoreCase)
                && parameter.In == ParameterLocation.Header);

            if (hasIdempotencyHeader)
            {
                return;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = IdempotencyHeaderName,
                In = ParameterLocation.Header,
                Required = true,
                Description = "Idempotency key required for POST requests.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                }
            });
        }
    }
}
