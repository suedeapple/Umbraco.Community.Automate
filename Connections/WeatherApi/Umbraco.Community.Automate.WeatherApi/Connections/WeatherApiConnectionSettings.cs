using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.WeatherApi.Connections;

/// <summary>
/// Stores the settings for a WeatherAPI.com connection in Umbraco Automate.
/// </summary>
public sealed class WeatherApiConnectionSettings
{
    /// <summary>
    /// The WeatherAPI.com API key used by this connection. Defaults to a reference to
    /// <c>Umbraco:Automate:Secrets:WeatherApi:ApiKey</c>, so a key stored in
    /// configuration works without typing it into the backoffice.
    /// </summary>
    [Field(
        Label = "API Key",
        Description = "The WeatherAPI.com API key.",
        IsSensitive = true,
        SortOrder = 1)]
    public string ApiKey { get; set; } = "$Umbraco:Automate:Secrets:WeatherApi:ApiKey";
}
