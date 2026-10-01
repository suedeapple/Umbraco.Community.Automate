namespace Umbraco.Community.Automate.WeatherApi.Connections;

/// <summary>
/// Shared validation for <see cref="WeatherApiConnectionSettings"/>, used by the connection
/// type (Test connection) and both actions, so they report the same message.
/// </summary>
public static class WeatherApiConnectionSettingsValidator
{
    /// <summary>Returns the first problem found, or <c>null</c> when the settings are valid.</summary>
    public static string? Validate(WeatherApiConnectionSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiKey))
            return "A WeatherAPI.com API key is required.";

        // Automate replaces a $-reference with the configured value; one that's still here
        // means the key isn't in configuration (or isn't on the allow-list).
        if (settings.ApiKey.TrimStart().StartsWith('$'))
            return $"The API key reference '{settings.ApiKey}' could not be resolved. Add the key to configuration at Umbraco:Community:Automate:WeatherApi:Secrets:ApiKey, or enter the key itself on the connection.";

        return null;
    }
}
