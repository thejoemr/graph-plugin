namespace graph_plugin.Models;

public struct GraphMessageInfo
{
    public Guid MessageId { get; set; }
    public IEnumerable<string> ToRecipients { get; init; }
    public IEnumerable<string> CcRecipients { get; init; }
    public string Subject { get; init; }
    public string Body { get; init; }
    public IReadOnlyDictionary<string, byte[]> Attachments { get; init; }
}