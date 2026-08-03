using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace NetrinAF.Api.Endpoints
{
    internal static class EndpointExtensions
    {
        internal static void AddEndpoints(this WebApplicationBuilder builder, Assembly assembly)
        {
            ServiceDescriptor[] endpointServiceDescriptors = assembly.DefinedTypes
                .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                    type.IsAssignableTo(typeof(IEndpoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
                .ToArray();

            builder.Services.TryAddEnumerable(endpointServiceDescriptors);
        }

        internal static void MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
        {
            IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

            IEndpointRouteBuilder endpointRouteBuilder = routeGroupBuilder is null ? app : routeGroupBuilder;

            foreach (IEndpoint endpoint in endpoints)
            {
                endpoint.MapEndpoint(endpointRouteBuilder);
            }
        }
    }
}
