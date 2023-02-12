using System.Collections;
using Newtonsoft.Json.Linq;

namespace graph_plugin.Models;

public struct GraphUserSearchResultDto : IEnumerable<JObject>
{
    public IEnumerable<JObject> Data { get; init; }
    public int PageSize { get; init; }
    public string? SkipToken { get; init; }

    public IEnumerator<JObject> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Data.GetEnumerator();
    }
}