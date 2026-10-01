namespace Umbraco.Community.Automate.Skoda.Configuration;

/// <summary>
/// Package options bound from <c>Umbraco:Automate:Variables:Skoda</c>, alongside the package's
/// other configuration in Umbraco Automate's shared Variables section.
/// </summary>
public class SkodaOptions
{
    public const string SectionName = SkodaConfiguration.VariablesPath;

    public Uri BaseUrl { get; set; } = new(SkodaConstants.BaseUrl);
}
