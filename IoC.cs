using graph_plugin.Clients;
using graph_plugin.Ports;
using graph_plugin.Security;
using Microsoft.Extensions.DependencyInjection;

namespace graph_plugin;

public static class IoC
{
    public static void AddGraph(this IServiceCollection services)
    {
        services.AddSingleton<AuthenticationManager>();
        services.AddSingleton<ApplicationOptions>();
        services.AddSingleton<GraphClientFactory>();
        services.AddTransient<IGraphUserClient>();
        services.AddTransient<IGraphMailClient>();

        services.AddMemoryCache();
    }
}