using Umbraco.Automate.Core.Settings;
using Umbraco.Community.Automate.DevTo.Configuration;

namespace Umbraco.Community.Automate.DevTo.Connections;

public sealed class DevToConnectionSettings
{
    public const string DefaultInstanceUrl = "https://dev.to";

    [Field(Label = "API Key",
        Description = "Your DEV API key, from [Settings → Extensions](https://dev.to/settings/extensions) on DEV. " +
                      "Defaults to the key in configuration at Umbraco:Automate:Secrets:DevTo:ApiKey; replace it with the key itself if you'd rather store it on the connection.",
        SortOrder = 0,
        IsSensitive = true)]
    public string ApiKey { get; set; } = DevToConfiguration.ApiKeyReference;

    [Field(Label = "Instance URL",
        Description = "Base URL of the Forem instance. Leave as https://dev.to unless you post to another Forem community.",
        SortOrder = 1,
        Group = "Advanced")]
    public string InstanceUrl { get; set; } = DefaultInstanceUrl;
}
