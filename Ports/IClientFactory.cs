using Microsoft.Graph;
using Microsoft.SharePoint.Client;

namespace graph_plugin.Ports;

public interface IClientFactory
{
    GraphServiceClient BuildNewToGraph(string username, string password, params string[] scopes);
    ClientContext BuildNewToSharepoint(Uri site, string username, string password, params string[] scopes);
}
