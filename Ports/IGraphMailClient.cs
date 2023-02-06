using graph_plugin.Models;
using Microsoft.Graph;

namespace graph_plugin.Ports;

public interface IGraphMailClient
{
    Task<Message?> FindMailByIdAsync(string messageId);
    Task SendAsync(GraphMessageInfo messageInfo);
    Task ReplayAsync(string messageToReplayId, GraphMessageInfo messageInfo);
}