using Microsoft.SharePoint.Client;

namespace graph_plugin.Ports;

public interface ISharepointClient
{
    ClientContext GetContext(Uri site);
    ClientContext GetContext(Uri site, string username, string password, params string[] scopes);
    IEnumerable<string> UploadFile(ClientContext context, string route, params (string Name, byte[] Content)[] files);
    IEnumerable<(string Name, byte[] Content)> DownloadFiles(ClientContext context, params string[] fileUrls);
}
