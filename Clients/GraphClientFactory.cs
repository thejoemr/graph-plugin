using System.Net.Http.Headers;
using graph_plugin.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace graph_plugin.Clients;

internal class GraphClientFactory
{
    // Configuration variable
    private readonly IConfiguration? _configuration;

    // Authentication Manager
    private readonly AuthenticationManager _authManager;

    // Constructor to initialize Configuration and Authentication Manager
    public GraphClientFactory(IServiceProvider serviceProvider)
    {
        _configuration = serviceProvider.GetService<IConfiguration>();
        _authManager = serviceProvider.GetService<AuthenticationManager>() ?? throw new NullReferenceException();
    }

    // Method to create a new GraphServiceClient
    public GraphServiceClient CreateNew(string username, string password, params string[] scopes)
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
}
