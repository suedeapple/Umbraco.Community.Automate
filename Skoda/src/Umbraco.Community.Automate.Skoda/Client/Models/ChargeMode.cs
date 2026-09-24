using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Community.Automate.Skoda.Client.Models;

public sealed record ChargeMode(
    [property: JsonPropertyName("chargeMode")] string Mode)
{
    [JsonExtensionData] public IDictionary<string, JsonElement>? AdditionalProperties { get; init; }
}
