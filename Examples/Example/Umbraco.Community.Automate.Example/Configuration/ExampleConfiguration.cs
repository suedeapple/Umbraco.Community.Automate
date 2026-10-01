namespace Umbraco.Community.Automate.Example.Configuration;

/// <summary>
/// The one place this package's configuration paths are defined. Values sit under Umbraco
/// Automate's shared <c>Umbraco:Automate:Secrets</c> (sensitive) and
/// <c>Umbraco:Automate:Variables</c> (non-sensitive) sections, which Automate resolves
/// <c>$</c> references from by default, so nothing needs registering. Nesting under the area
/// name keeps the package's keys together.
/// </summary>
public static class ExampleConfiguration
{
    public const string SecretsPath = "Umbraco:Automate:Secrets:Example";

    /// <summary>
    /// The value a new connection's API key field starts with, so a key stored in configuration
    /// (appsettings, environment variables, user secrets) is used without typing anything.
    /// </summary>
    public const string ApiKeyReference = "$" + SecretsPath + ":ApiKey";
}
