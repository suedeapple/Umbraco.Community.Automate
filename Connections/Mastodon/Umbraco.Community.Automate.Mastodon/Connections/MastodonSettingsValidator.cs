namespace Umbraco.Community.Automate.Mastodon.Connections;

/// <summary>
/// Shared validation for <see cref="MastodonSettings"/>, used by both the
/// connection type (Test connection) and the send-post action.
/// </summary>
public static class MastodonSettingsValidator
{
    /// <summary>
    /// Returns an error message describing the first problem found, or <c>null</c> when the settings are valid.
    /// </summary>
    public static string? Validate(MastodonSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.InstanceUrl))
            return "Instance URL is required.";

        if (string.IsNullOrWhiteSpace(settings.AccessToken))
            return "Access token is required.";

        // Automate replaces a $-reference with the configured value; one that's still here
        // means the value isn't in configuration.
        if (IsUnresolvedReference(settings.InstanceUrl))
            return $"The instance URL reference '{settings.InstanceUrl}' could not be resolved. Add the URL to configuration at Umbraco:Community:Automate:Mastodon:Variables:InstanceUrl, or enter it on the connection.";

        if (IsUnresolvedReference(settings.AccessToken))
            return $"The access token reference '{settings.AccessToken}' could not be resolved. Add the token to configuration at Umbraco:Community:Automate:Mastodon:Secrets:AccessToken, or enter it on the connection.";

        if (!Uri.TryCreate(settings.InstanceUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return "Instance URL must be an absolute URL starting with http:// or https:// (e.g. https://mastodon.social).";
        }

        return null;
    }

    private static bool IsUnresolvedReference(string value) => value.TrimStart().StartsWith('$');
}
