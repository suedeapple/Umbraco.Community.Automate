namespace Umbraco.Community.Automate.DevTo.Configuration;

public static class DevToConfiguration
{
    public const string SectionPath = "Umbraco:Community:Automate:DevTo";
    public const string VariablesPath = SectionPath + ":Variables";
    public const string SecretsPath = SectionPath + ":Secrets";

    /// <summary>The reference new connections start with, so a key in configuration is used without any typing.</summary>
    public const string ApiKeyReference = "$" + SecretsPath + ":ApiKey";
}
