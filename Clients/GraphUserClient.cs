using graph_plugin.Models;
using graph_plugin.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace graph_plugin.Clients;

// Class that implements the IGraphUserClient interface and provides the necessary methods to interact with Microsoft Graph API 
internal class GraphUserClient : IGraphUserClient
{
    // Private field to store the GraphServiceClient instance
    private readonly GraphServiceClient _graphClient;

    // Constructor that retrieves the necessary data from the configuration and creates the GraphServiceClient instance
    public GraphUserClient(IServiceProvider serviceProvider)
    {
        // Retrieve the configuration data
        var configuration = serviceProvider.GetService<IConfiguration>() ?? throw new NullReferenceException();
        var clientFactory = serviceProvider.GetService<IClientFactory>() ?? throw new NullReferenceException();

        // Get the values of the `usr` and `pwd` parameters from the configuration.
        var usr = configuration.GetValue<string>("your_username_azureAD") ?? throw new NullReferenceException();
        var pwd = configuration.GetValue<string>("your_password_azureAD") ?? throw new NullReferenceException();

        // Create a new `GraphServiceClient` instance with the given `username`, `password`, and `scopes`.
        _graphClient = clientFactory.BuildNewToGraph(username: usr, password: pwd, scopes: new string[]
        {
            "User.Read"
        });
    }

    // Method to find a user by their email address
    public async Task<User?> FindUserByMailAsync(string mail)
    {
        try
        {
            // Make a request to the Microsoft Graph API to retrieve the user data
            return await _graphClient.Users[mail].Request().GetAsync();
        }
        catch (Exception)
        {
            // If the mail is not part of the azure active directory users, return null as default
            return null;
        }
    }

    // Method to search for users by a text match
    public async Task<GraphUserSearchResultDto> GetUsersByMatchAsync(string? textToMatch = null, int pageSize = 10, string? skipToken = null)
    {
        // List of query options to use when making the request to the Microsoft Graph API
        var queryOptions = new List<QueryOption>
            {
                new QueryOption(name: "$top", value: pageSize.ToString())
            };

        // Add the $skipToken option to the query options if it is provided
        if (!string.IsNullOrEmpty(skipToken))
            queryOptions.Add(new QueryOption(name: "$skipToken", value: skipToken));

        // Array of filter conditions to use when searching for users
        var filterConditions = new string[]
        {
                $"startswith(displayName, '{textToMatch}')",
                $"startswith(givenName, '{textToMatch}')",
                $"startswith(surname, '{textToMatch}')",
                $"startswith(mail, '{textToMatch}')",
        };

        // Make a request to the Microsoft Graph API to retrieve the matching users
        var request = _graphClient.Users.Request(queryOptions);
        var users = string.IsNullOrEmpty(textToMatch) ? await request.GetAsync() : await request.Filter(string.Join(" or ", filterConditions)).GetAsync();

        // Retrieve the next page token, if available
        skipToken = users.NextPageRequest?.QueryOptions?.FirstOrDefault(qo => string.Equals("$skipToken", qo.Name, StringComparison.InvariantCultureIgnoreCase))?.Value;

        // Return the result of the search
        return new GraphUserSearchResultDto
        {
            Data = users.Select(user => new JObject
            {
                ["mail"] = user.Mail,
                ["name"] = user.DisplayName
            }),
            PageSize = pageSize,
            SkipToken = skipToken
        };
    }
}