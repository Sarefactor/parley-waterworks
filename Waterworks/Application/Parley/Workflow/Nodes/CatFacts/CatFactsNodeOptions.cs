using Parley.Workflows.Nodes;
using System.Text.Json.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace Application.Parley.Workflow.Nodes.CatFacts;

[ExportTsClass]
public class CatFactsNodeOptions : ParleyNodeOptions
{
    [JsonInclude]
    [JsonPropertyName("maxLength")]
    public int MaxLength { get; set; } = default!;

    [JsonInclude]
    [JsonPropertyName("targetKey")]
    public string TargetKey { get; set; } = string.Empty;
}
