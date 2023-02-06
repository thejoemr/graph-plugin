using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace graph_plugin.Security;

// This class is used to store application options for authentication.
internal class ApplicationOptions
{
    // A field of type PublicClientApplicationOptions
    public readonly PublicClientApplicationOptions PublicOptions;

    // A field of type ConfidentialClientApplicationOptions
    public readonly ConfidentialClientApplicationOptions ConfidentialOptions;

    // A constructor that takes an IConfiguration object as a parameter
    public ApplicationOptions(IConfiguration configuration)
    {
        // Read the clientId and tenantId from the configuration
        var clientId = configuration.GetValue<string>("<your_clientId>");
        var tenantId = configuration.GetValue<string>("<your_tenantId>");

        // Initialize the PublicOptions field with the clientId and tenantId
        PublicOptions = new PublicClientApplicationOptions
        {
            ClientId = clientId,
            TenantId = tenantId
        };

        // Initialize the ConfidentialOptionss field with the clientId and tenantId
        ConfidentialOptions = new ConfidentialClientApplicationOptions
        {
            ClientId = clientId,
            TenantId = tenantId
        };
    }
}