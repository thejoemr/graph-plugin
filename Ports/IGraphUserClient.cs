using graph_plugin.Models;
using Microsoft.Graph;

namespace graph_plugin.Ports;

public interface IGraphUserClient
{
    Task<User> FindUserByMailAsync(string mail);
    Task<GraphUserSearchResult> GetUsersByMatchAsync(string? textToMatch = null, int pageSize = 10, string? skipToken = null);
}