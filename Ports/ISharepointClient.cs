using Microsoft.SharePoint.Client;

namespace graph_plugin.Ports;

public interface ISharepointClient
{
    ClientContext GetContext();
    ClientContext GetContext(Uri site);
    ClientContext GetContext(Uri site, string username, string password);
    IEnumerable<string> UploadFile(ClientContext context, string route, params (string Name, byte[] Content)[] files);
    IEnumerable<(string Name, byte[] Content)> DownloadFile(ClientContext context, params string[] fileUrls);
}
