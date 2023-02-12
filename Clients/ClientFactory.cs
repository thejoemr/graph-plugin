using System.Net.Http.Headers;
using graph_plugin.Ports;
using graph_plugin.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;

namespace graph_plugin.Clients;

internal class ClientFactory : IClientFactory
{
    // Configuration variable
    private readonly IConfiguration _configuration;

    // Authentication Manager
    private readonly AuthenticationManager _authManager;

    // Constructor to initialize Configuration and Authentication Manager
    public ClientFactory(IServiceProvider serviceProvider)
    {
        _configuration = serviceProvider.GetService<IConfiguration>() ?? throw new NullReferenceException();
        _authManager = serviceProvider.GetService<AuthenticationManager>() ?? throw new NullReferenceException();
    }

    // Method to create a new GraphServiceClient
    public GraphServiceClient BuildNewToGraph(string username, string password, params string[] scopes)
    {
        var client = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage =>
        {
            // Get the result of authentication by username and password
            var authResult = _authManager.AuthByUsernamePasswordAsync(username, password, scopes).GetAwaiter().GetResult();

            // Add authorization header with the access token
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult?.AccessToken);

            // Return a completed task
            return Task.CompletedTask;
        }));

        // Return the created GraphServiceClient
        return client;
    }

    // Method to create a new ClientContext
    public ClientContext BuildNewToSharepoint(Uri site, string username, string password, params string[] scopes)
    {
        // Crea un contexto de cliente de SharePoint
        var context = new ClientContext(site);

        // Agrega el token de autorización a la cabecera de la solicitud
        context.ExecutingWebRequest += (sender, e) =>
        {
            // Get the result of authentication by username and password
            var authResult = _authManager.AuthInWebByUsernamePasswordAsync(site, username, password, scopes).GetAwaiter().GetResult();

            // Add authorization header with the access token
            e.WebRequestExecutor.RequestHeaders["Authorization"] = $"Bearer {authResult?.AccessToken}";
        };

        return context;
    }
}
