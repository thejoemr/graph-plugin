using graph_plugin.Models;
using graph_plugin.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace graph_plugin.Clients;

// This class is an implementation of the `IGraphMailClient` interface.
internal class GraphMailClient : IGraphMailClient
{
    // An instance of `GraphServiceClient` is created to interact with the Microsoft Graph API.
    private readonly GraphServiceClient _graphClient;
    // A property that holds the property namespace for the custom message Id.
    private readonly Guid _propertyNamespace = Guid.Parse("75CF8950-C01A-4D68-850D-13E7E49C12C0");

    // A constructor that takes an `IServiceProvider` as an argument to retrieve dependencies.
    public GraphMailClient(IServiceProvider serviceProvider)
    {
        // Get the configuration from the service provider.
        var configuration = serviceProvider.GetService<IConfiguration>();
        // Get the `GraphClientFactory` from the service provider or throw an exception if it's null.
        var clientFactory = serviceProvider.GetService<GraphClientFactory>() ?? throw new NullReferenceException();

        // Get the values of the `usr` and `pwd` parameters from the configuration.
        var usr = configuration?.GetValue<string>("your_username_azureAD");
        var pwd = configuration?.GetValue<string>("your_password_azureAD");

        // Create a new `GraphServiceClient` instance with the given `username`, `password`, and `scopes`.
        _graphClient = clientFactory.CreateNew(username: usr ?? null!, password: pwd ?? null!, scopes: new string[]
        {
            "Mail.Send",
            "Mail.ReadWrite"
        });
    }

    // A method to find an email by its Id.
    public async Task<Message?> FindMailByIdAsync(string messageId)
    {
        try
        {
            // Try to get the message directly.
            return await _graphClient.Me.Messages[messageId].Request().GetAsync();
        }
        catch (Exception)
        {
            // If the previous step fails, search for the message using a filter with the custom message Id.
            var searchMessages = await _graphClient.Me.Messages.Request()
            .Filter($"singleValueExtendedProperties/Any(ep: ep/id eq 'String {_propertyNamespace:B} Name MessageCustomId' and ep/value eq '{messageId}')")
            .Expand($"singleValueExtendedProperties($filter=id eq 'String {_propertyNamespace:B} Name MessageCustomId')")
            .GetAsync();

            // Return the first result from the search or `null` if there are no results.
            return searchMessages.Any() ? searchMessages.First() : null;
        }
    }

    // A method to send an email.
    public async Task SendAsync(GraphMessageInfo messageInfo)
    {
        // Create a new instance of the `Message` class.
        var message = new Message
        {
            Subject = messageInfo.Subject,
            Body = new ItemBody
            {
                Content = messageInfo.Body,
                ContentType = BodyType.Html
            },
            ToRecipients = messageInfo.ToRecipients.Select(addr => new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = addr
                }
            }),
            CcRecipients = messageInfo.CcRecipients.Select(addr => new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = addr
                }
            }),
            Attachments = new MessageAttachmentsCollectionPage(),
            SingleValueExtendedProperties = new MessageSingleValueExtendedPropertiesCollectionPage()
        };

        // Adding custom property to identify the message on graph
        message.SingleValueExtendedProperties.Add(new SingleValueLegacyExtendedProperty
        {
            Id = $"String {_propertyNamespace} Name MessageCustomId",
            Value = messageInfo.MessageId.ToString()
        });

        // Adding the attachmentes 
        foreach (var file in messageInfo.Attachments)
            message.Attachments.Add(new FileAttachment
            {
                Name = file.Key,
                ContentBytes = file.Value,
                ContentType = "application/octet-stream"
            });

        // Sending the message
        await _graphClient.Me.SendMail(message, SaveToSentItems: true).Request().PostAsync();
    }

    // A method to send an email as replay.
    public async Task ReplayAsync(string messageToReplayId, GraphMessageInfo messageInfo)
    {
        // Create a new instance of the `Message` class.
        var message = new Message
        {
            Subject = messageInfo.Subject,
            Body = new ItemBody
            {
                Content = messageInfo.Body,
                ContentType = BodyType.Html
            },
            ToRecipients = messageInfo.ToRecipients.Select(addr => new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = addr
                }
            }),
            CcRecipients = messageInfo.CcRecipients.Select(addr => new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = addr
                }
            }),
            Attachments = new MessageAttachmentsCollectionPage(),
            SingleValueExtendedProperties = new MessageSingleValueExtendedPropertiesCollectionPage()
        };

        // Adding custom property to identify the message on graph
        message.SingleValueExtendedProperties.Add(new SingleValueLegacyExtendedProperty
        {
            Id = $"String {_propertyNamespace} Name MessageCustomId",
            Value = messageInfo.MessageId.ToString()
        });

        // Adding the attachmentes 
        foreach (var file in messageInfo.Attachments)
            message.Attachments.Add(new FileAttachment
            {
                Name = file.Key,
                ContentBytes = file.Value,
                ContentType = "application/octet-stream"
            });

        // Finding the message to replay
        var messageToReplay = await FindMailByIdAsync(messageToReplayId) ?? throw new NullReferenceException();

        // Send the replay message
        await _graphClient.Me.Messages[messageToReplay.Id].Reply(message).Request().PostAsync();
    }
}