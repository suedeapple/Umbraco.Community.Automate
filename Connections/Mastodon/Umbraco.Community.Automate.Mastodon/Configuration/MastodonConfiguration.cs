namespace Umbraco.Community.Automate.Mastodon.Configuration;

public static class MastodonConfiguration
{
    public const string SectionPath = "Umbraco:Community:Automate:Mastodon";
    public const string VariablesPath = SectionPath + ":Variables";
    public const string SecretsPath = SectionPath + ":Secrets";

    /// <summary>The references new connections start with, so values in configuration are used without any typing.</summary>
    public const string InstanceUrlReference = "$" + VariablesPath + ":InstanceUrl";
    public const string AccessTokenReference = "$" + SecretsPath + ":AccessToken";
}
