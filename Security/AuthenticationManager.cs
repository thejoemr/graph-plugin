using System.Text;
using graph_plugin.Ports;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace graph_plugin.Security;

internal class AuthenticationManager : IAuthenticationManager
{
    // Fields for configuration and cache service
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    
    // Fields for public and confidential client applications
    private readonly IPublicClientApplication _publicApplication;
    private readonly IConfidentialClientApplication _confidentialApplication;

    // Constructor to initialize configuration, cache, and client applications
    public AuthenticationManager(IServiceProvider serviceProvider)
    {
        // Get the configuration and cache services
        _configuration = serviceProvider.GetService<IConfiguration>() ?? throw new NullReferenceException();
        _cache = serviceProvider.GetService<IMemoryCache>() ?? throw new NullReferenceException();
        
        // Read the clientSecret, clientId, tenantId from the configuration
        var clientId = _configuration.GetValue<string>("<your_clientId>");
        var tenantId = _configuration.GetValue<string>("<your_tenantId>");
        var clientSecret = _configuration.GetValue<string>("<your_clientSecret>");

        // Get the application options and initialize the public and confidential client applications
        _publicApplication = PublicClientApplicationBuilder.CreateWithApplicationOptions(new PublicClientApplicationOptions
        {
            ClientId = clientId,
            TenantId = tenantId
        }).Build();

        _confidentialApplication = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(new ConfidentialClientApplicationOptions
        {
            ClientId = clientId,
            TenantId = tenantId
        }).WithClientSecret(clientSecret).Build();
    }

    // Method to authenticate by username and password
    public async Task<AuthenticationResult> AuthByUsernamePasswordAsync(string username, string password, params string[] scopes)
    {
        //  Generates the caché key, this will be unique by user and scopes
        var cacheKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username};{string.Join(',', scopes)}"));

        // Check if the authentication result is in cache and if it's close to expiration
        if (_cache.TryGetValue<AuthenticationResult>(cacheKey, out var authResult) && authResult is not null)
        {
            var timeToExpire = authResult.ExpiresOn.UtcDateTime - DateTime.UtcNow;
            if (timeToExpire.TotalMinutes <= 2)
            {
                // Re-acquire the authentication result and update the cache
                authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
                _cache.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
                return authResult;
            }

            // Return the cached authentication result if it's still valid
            return authResult;
        }

        // Acquire the authentication result and add it to cache
        authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
        _cache.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
        return authResult;
    }

    // Method to authenticate by username and password in a web
    public async Task<AuthenticationResult> AuthInWebByUsernamePasswordAsync(Uri web, string username, string password, params string[] scopes)
    {
        //  Generates the caché key, this will be unique by web, user and scopes
        var cacheKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{web};{username};{string.Join(',', scopes)}"));

        // Check if the authentication result is in cache and if it's close to expiration
        if (_cache.TryGetValue<AuthenticationResult>(cacheKey, out var authResult) && authResult is not null)
        {
            var timeToExpire = authResult.ExpiresOn.UtcDateTime - DateTime.UtcNow;
            if (timeToExpire.TotalMinutes <= 2)
            {
                // Re-acquire the authentication result and update the cache
                authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
                _cache.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
                return authResult;
            }

            // Return the cached authentication result if it's still valid
            return authResult;
        }

        // Acquire the authentication result and add it to cache
        authResult = await _publicApplication.AcquireTokenByUsernamePassword(scopes, username, password).ExecuteAsync();
        _cache.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
        return authResult;
    }


    // Method to authenticate by a previous token
    public async Task<AuthenticationResult> AuthOnBehalfOfAsync(string token, params string[] scopes)
    {
        //  Generates the caché key, this will be unique by token and scopes
        var cacheKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{token};{string.Join(',', scopes)}"));

        // Acquire the authentication result
        var userAssertion = new UserAssertion(token, "urn:ietf:params:oauth:grant-type:jwt-bearer");
        
        // Check if the authentication result is in cache and if it's close to expiration
        if (_cache.TryGetValue<AuthenticationResult>(cacheKey, out var authResult) && authResult is not null)
        {
            var timeToExpire = authResult.ExpiresOn.UtcDateTime - DateTime.UtcNow;
            if (timeToExpire.TotalMinutes <= 2)
            {
                // Re-acquire the authentication result and update the cache
                authResult = await _confidentialApplication.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
                _cache.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
                return authResult;
            }

            // Return the cached authentication result if it's still valid
            return authResult;
        }

        // Acquire the authentication result and add it to cache
        authResult = await _confidentialApplication.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
        _cache.Set(cacheKey, authResult, TimeSpan.FromMinutes(30));
        return authResult;
    }
}