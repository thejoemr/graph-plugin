using graph_plugin.Clients;
using graph_plugin.Ports;
using graph_plugin.Security;
using Microsoft.Extensions.DependencyInjection;

namespace graph_plugin;

public static class IoC
{
    public static void AddGraph(this IServiceCollection services)
    {
        services.AddSingleton<IAuthenticationManager, AuthenticationManager>();
        services.AddSingleton<IClientFactory, ClientFactory>();
        services.AddTransient<IGraphUserClient, GraphUserClient>();
        services.AddTransient<IGraphMailClient, GraphMailClient>();
        services.AddTransient<ISharepointClient, SharepointClient>();

        services.AddMemoryCache();
    }
}