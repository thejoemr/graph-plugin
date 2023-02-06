using Newtonsoft.Json.Linq;

namespace graph_plugin.Models;

public struct GraphUserSearchResult
{
    public IEnumerable<JObject> Data { get; init; }
    public int PageSize { get; init; }
    public string? SkipToken { get; init; }
}
