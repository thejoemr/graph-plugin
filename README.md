# graph-plugin
Library to connect .NET with MS Graph

# Setup
## GraphMailClient (./Clients/GraphMailClient.cs)
Configure the class with the [scopes](https://learn.microsoft.com/en-us/graph/permissions-reference#all-permissions-and-ids) that you will use in your app. By default, "Mail.Send" and "Mail.ReadWrite" are set.

The MS Graph client needs a username and password to authenticate the app and manage cache tokens. These values must be set in the constructor of the class and stored in the application configuration.

## GraphUserClient (./Clients/GraphUserClient.cs)
Configure the class with the [scopes](https://learn.microsoft.com/en-us/graph/permissions-reference#all-permissions-and-ids) that you will use in your app. By default, "User.Read" is set.

The MS Graph client needs a username and password to authenticate the app and manage cache tokens. These values must be set in the constructor of the class and stored in the application configuration.

## ApplicationOptions (./Security/ApplicationOptions.cs)
Configure the clientId (application ID) and tenantId of your Azure Application.

## AuthenticationManager (./Security/AuthenticationManager.cs)
The plugin works with cache memory, meaning that all authentication processes are managed to reduce the number of requests to access an Azure Graph resource. Each token is validated and stored using the username or web and the scopes requested. Before accessing a token, it is validated to determine if a new token needs to be requested.

## IoC (./IoC.cs)
Inject the module on your app using Dependency Injection

```
services.AddGraph();
```
## Usage
Only you can use two services (mail and users), you just have to inject the dependencys on your service.
```
public class MyCustomService
{
    private readonly IGraphMailClient graphMailClient;
    private readonly IGraphUserClient graphUserClient;

    public MyCustomService(IGraphMailClient graphMailClient, IGraphUserClient graphUserClient)
    {
        // Get the services by dependency injection
        this.graphMailClient = graphMailClient;
        this.graphUserClient = graphUserClient;
    }
}
```
