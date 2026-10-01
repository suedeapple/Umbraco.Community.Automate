namespace Umbraco.Community.Automate.WeatherApi.Configuration;

/// <summary>
/// Configuration section paths for values the WeatherAPI.com connection can reference with
/// Umbraco Automate's <c>$</c> syntax. Registered on Automate's allow-list in
/// <see cref="Composers.WeatherApiComposer"/>.
/// </summary>
public static class WeatherApiConfiguration
{
    public const string SectionPath = "Umbraco:Community:Automate:WeatherApi";
    public const string VariablesPath = SectionPath + ":Variables";
    public const string SecretsPath = SectionPath + ":Secrets";

    /// <summary>The reference new connections start with, so a key in configuration is used without any typing.</summary>
    public const string ApiKeyReference = "$" + SecretsPath + ":ApiKey";
}
