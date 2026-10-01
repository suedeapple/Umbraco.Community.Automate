namespace Umbraco.Community.Automate.DevTo.Configuration;

/// <summary>
/// Where DevTo's non-secret values live in configuration: a <c>DevTo</c> key inside Umbraco
/// Automate's shared <c>Umbraco:Automate:Variables</c> section. The Markdown preview only resolves
/// references under this path, so it never reads secrets or other configuration.
/// </summary>
public static class DevToConfiguration
{
    public const string VariablesPath = "Umbraco:Automate:Variables:DevTo";
}
