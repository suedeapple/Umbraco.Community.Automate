using Umbraco.Automate.Core.Settings;
using Umbraco.Community.Automate.Example.Configuration;

namespace Umbraco.Community.Automate.Example.Connections;

/// <summary>The settings stored on an Example connection.</summary>
public sealed class ExampleConnectionSettings
{
    // Defaults to a reference to configuration, so a new connection opens pre-filled and a key
    // stored in appsettings just works. The description doesn't repeat the path: the field
    // already shows it.
    [Field(
        Label = "API key",
        Description = "Any token works with httpbin.org; a real service would say where to get one.",
        IsSensitive = true,
        SortOrder = 0)]
    public string ApiKey { get; set; } = ExampleConfiguration.ApiKeyReference;
}
