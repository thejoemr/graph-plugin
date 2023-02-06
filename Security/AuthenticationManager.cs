using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace graph_plugin.Security;

internal class AuthenticationManager
{
    // Fields for configuration and cache service
    private readonly IConfiguration? _configuration;
    private readonly IMemoryCache? _cache;
    
    // Fields for public and confidential client applications
    private readonly IPublicClientApplication _publicApplication;
    private readonly IConfidentialClientApplication _confidentialApplication;

    // Constructor to initialize configuration, cache, and client applications
    public AuthenticationManager(IServiceProvider serviceProvider)
    {
        // Get the configuration and cache services
        _configuration = serviceProvider.GetService<IConfiguration>();
        _cache = serviceProvider.GetService<IMemoryCache>();

        // Get the application options and initialize the public and confidential client applications
        var applicationOptions = serviceProvider.GetService<ApplicationOptions>() ?? throw new NullReferenceException();
        _publicApplication = PublicClientApplicationBuilder.CreateWithApplicationOptions(applicationOptions.PublicOptions).Build();
        _confidentialApplication = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(applicationOptions.ConfidentialOptions).WithClientSecret(_configuration?.GetValue<string>("<your_clientSecret>")).Build();
    }

    // Method to authenticate by username and password
    public async Task<AuthenticationResult> AuthByUsernamePasswordAsync(string username, string password, params string[] scopes)
    {
        var cacheKey = $"{username};{string.Join(',', scopes)}";
        cacheKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cacheKey));

        // Check if the authentication result is in cache and if it's close to expiration
        if ((_cache?.TryGetValue<AuthenticationResult>(cacheKey, out var authResult) ?? false) && authResult is not null)
        {
            var timeToExpire = authResult.ExpiresOn.UtcDateTime - DateTime.UtcNow;
            if (timeToExpire.TotalMinutes <= 2)
            {
                // Re-acquire the authentication result and update the cache
                authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
                _cache?.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
                return authResult;
            }

            // Return the cached authentication result if it's still valid
            return authResult;
        }

        // Acquire the authentication result and add it to cache
        authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
        _cache?.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
        return authResult;
    }

    // Method to authenticate by username and password in a web
    public async Task<AuthenticationResult> AuthInWebByUsernamePasswordAsync(Uri web, string username, string password, params string[] scopes)
    {
        var cacheKey = $"{web};{username};{string.Join(',', scopes)}";
        cacheKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cacheKey));

        // Check if the authentication result is in cache and if it's close to expiration
        if ((_cache?.TryGetValue<AuthenticationResult>(cacheKey, out var authResult) ?? false) && authResult is not null)
        {
            var timeToExpire = authResult.ExpiresOn.UtcDateTime - DateTime.UtcNow;
            if (timeToExpire.TotalMinutes <= 2)
            {
                // Re-acquire the authentication result and update the cache
                authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
                _cache?.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
                return authResult;
            }

            // Return the cached authentication result if it's still valid
            return authResult;
        }

        // Acquire the authentication result and add it to cache
        authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
        _cache?.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
        return authResult;
    }


    // Method to authenticate by a previous token
    public async Task<AuthenticationResult> AuthOnBehalfOfAsync(string token, params string[] scopes)
    {
        var cacheKey = $"{token};{string.Join(',', scopes)}";
        cacheKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cacheKey));

        // Acquire the authentication result
        var userAssertion = new UserAssertion(token, "urn:ietf:params:oauth:grant-type:jwt-bearer");
        
        // Check if the authentication result is in cache and if it's close to expiration
        if ((_cache?.TryGetValue<AuthenticationResult>(cacheKey, out var authResult) ?? false) && authResult is not null)
        {
            var timeToExpire = authResult.ExpiresOn.UtcDateTime - DateTime.UtcNow;
            if (timeToExpire.TotalMinutes <= 2)
            {
                // Re-acquire the authentication result and update the cache
                authResult = await _confidentialApplication.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
                _cache?.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
                return authResult;
            }

            // Return the cached authentication result if it's still valid
            return authResult;
        }

        // Acquire the authentication result and add it to cache
        authResult = await _confidentialApplication.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
        _cache?.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
        return authResult;
    }
}