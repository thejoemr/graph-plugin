using Microsoft.Identity.Client;

namespace graph_plugin.Ports;

public interface IAuthenticationManager
{
    Task<AuthenticationResult> AuthByUsernamePasswordAsync(string username, string password, params string[] scopes);
    Task<AuthenticationResult> AuthInWebByUsernamePasswordAsync(Uri web, string username, string password, params string[] scopes);
    Task<AuthenticationResult> AuthOnBehalfOfAsync(string token, params string[] scopes);
}