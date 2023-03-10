using graph_plugin.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;

namespace graph_plugin.Clients;

internal class SharepointClient : ISharepointClient
{
    private readonly IConfiguration _configuration;
    private readonly IClientFactory _clientFactory;

    public SharepointClient(IServiceProvider serviceProvider)
    {
        // Retrieve the configuration data
        _configuration = serviceProvider.GetService<IConfiguration>() ?? throw new NullReferenceException();
        _clientFactory = serviceProvider.GetService<IClientFactory>() ?? throw new NullReferenceException();
    }

    public ClientContext GetContext()
    {
        // Get the values of the `usr` and `pwd` parameters from the configuration.
        var site = _configuration.GetValue<string>("your_default_site") ?? throw new NullReferenceException();
        var usr = _configuration.GetValue<string>("your_username_azureAD") ?? throw new NullReferenceException();
        var pwd = _configuration.GetValue<string>("your_password_azureAD") ?? throw new NullReferenceException();

        // Create a new `ClientContext` instance with the given `username`, `password`, and `scopes`.
        return GetContext(new Uri(site), usr, pwd);
    }

    public ClientContext GetContext(Uri site)
    {
        // Get the values of the `usr` and `pwd` parameters from the configuration.
        var usr = _configuration.GetValue<string>("your_username_azureAD") ?? throw new NullReferenceException();
        var pwd = _configuration.GetValue<string>("your_password_azureAD") ?? throw new NullReferenceException();

        // Create a new `ClientContext` instance with the given `username`, `password`, and `scopes`.
        return GetContext(site, usr, pwd);
    }

    public ClientContext GetContext(Uri site, string username, string password)
    {
        // Create a new `ClientContext` instance with the given `username`, `password`, and `scopes`.
        return _clientFactory.BuildNewToSharepoint(site, username, password, scopes: new string[]
        {
            "Sites.Read.All"
        });
    }

    public IEnumerable<string> UploadFile(ClientContext context, string route, params (string Name, byte[] Content)[] files)
    {
        var output = new List<File>();

        // Create the file route
        var currentFolder = context.Web.DefaultDocumentLibrary().RootFolder; // Start from the root folder

        // Split the route by segments and adding all as a new folder
        foreach (var segment in route.Split('/'))
            currentFolder = currentFolder.Folders.Add(segment);

        // Upload all files
        foreach ((string Name, byte[] Content) in files)
        {
            // Adding the file in the folder
            var file = currentFolder.Files.Add(new FileCreationInformation
            {
                Content = Content,
                Url = Name,
                Overwrite = true
            });

            // Loading the server relative url
            context.Load(file, f => f.ServerRelativeUrl);

            // Adding the file in the output list
            output.Add(file);
        }

        // Exwcuting the actions
        context.ExecuteQuery();

        // Return the server routes of the files
        return output.Select(file => file.ServerRelativeUrl);
    }

    public IEnumerable<(string Name, byte[] Content)> DownloadFile(ClientContext context, params string[] fileUrls)
    {
        var output = new List<(string Name, byte[] Content)>();

        // Loop through each file URL
        foreach (var fileUrl in fileUrls)
        {
            // Get the file name from the URL
            var fileName = Path.GetFileName(fileUrl);

            // Get the file from SharePoint
            var file = context.Web.GetFileByServerRelativeUrl(fileUrl);
            context.Load(file);
            context.ExecuteQuery();

            // Get the file content
            var fileInformation = file.OpenBinaryStream();

            // Use a memory stream to store the binary data
            using var memoryStream = new MemoryStream();
            // Copy the binary data from the file stream to the memory stream
            fileInformation.Value.CopyTo(memoryStream);
            // Adding the file name and content as a tuple
            output.Add((fileName, memoryStream.ToArray()));
        }

        return output;
    }
}