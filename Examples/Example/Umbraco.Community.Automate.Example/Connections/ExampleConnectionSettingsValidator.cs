using Umbraco.Community.Automate.Example.Configuration;

namespace Umbraco.Community.Automate.Example.Connections;

/// <summary>
/// Shared validation for <see cref="ExampleConnectionSettings"/>, used by the connection type
/// (Test connection) and every action, so they all report the same message.
/// </summary>
public static class ExampleConnectionSettingsValidator
{
    /// <summary>Returns the first problem found, or <c>null</c> when the settings are valid.</summary>
    public static string? Validate(ExampleConnectionSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiKey))
            return "An API key is required.";

        // Automate resolves $-references before settings reach this code, and reports a missing
        // key itself; this is a safety net for a reference that arrives unresolved anyway.
        if (settings.ApiKey.TrimStart().StartsWith('$'))
            return $"The API key reference '{settings.ApiKey}' could not be resolved. Add the key to configuration at {ExampleConfiguration.SecretsPath}:ApiKey, or enter it on the connection.";

        return null;
    }
}
