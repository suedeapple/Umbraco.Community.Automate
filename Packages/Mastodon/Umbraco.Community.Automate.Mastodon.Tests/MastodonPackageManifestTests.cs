using System.Text.Json;
using Xunit;

namespace Umbraco.Community.Automate.Mastodon.Tests;

/// <summary>
/// wwwroot/umbraco-package.json registers the package's backoffice extensions. Nothing builds or
/// checks it, so a wrong path or a missing file would only show up as a broken icon.
/// </summary>
public class MastodonPackageManifestTests
{
    private static JsonElement IconsExtension()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(MastodonPackagePaths.Wwwroot, "umbraco-package.json")));
        return Assert.Single(doc.RootElement.GetProperty("extensions").EnumerateArray(),
            e => e.GetProperty("type").GetString() == "icons").Clone();
    }

    [Fact]
    public void Registers_an_icons_extension()
        => Assert.Equal("UmbracoCommunityAutomateMastodon.Icons", IconsExtension().GetProperty("alias").GetString());

    [Fact]
    public void Icons_extension_points_at_files_that_exist()
    {
        // Must match StaticWebAssetBasePath in the csproj, or the backoffice 404s on the module.
        Assert.Equal("/App_Plugins/UmbracoCommunityAutomateMastodon/icons/icons.js", IconsExtension().GetProperty("js").GetString());

        Assert.True(File.Exists(Path.Combine(MastodonPackagePaths.Wwwroot, "icons", "icons.js")));
        Assert.True(File.Exists(Path.Combine(MastodonPackagePaths.Wwwroot, "icons", "mastodon.icon.js")));
    }
}
