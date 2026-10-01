namespace Umbraco.Community.Automate.Skoda.Configuration;

/// <summary>
/// Configuration section paths for values a Škoda connection can reference with Umbraco
/// Automate's <c>$</c> syntax. Registered on Automate's allow-list in the composer.
/// </summary>
public static class SkodaConfiguration
{
    public const string SectionPath = SkodaOptions.SectionName;
    public const string VariablesPath = SectionPath + ":Variables";
    public const string SecretsPath = SectionPath + ":Secrets";

    /// <summary>The reference new connections start with, so a key in configuration is used without any typing.</summary>
    public const string ApiKeyReference = "$" + SecretsPath + ":ApiKey";
}
