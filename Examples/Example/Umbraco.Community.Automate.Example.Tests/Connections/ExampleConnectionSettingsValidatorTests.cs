using Umbraco.Community.Automate.Example.Connections;
using Xunit;

namespace Umbraco.Community.Automate.Example.Tests.Connections;

public class ExampleConnectionSettingsValidatorTests
{
    [Fact]
    public void New_connections_default_to_the_configuration_reference()
        => Assert.Equal("$Umbraco:Automate:Secrets:Example:ApiKey", new ExampleConnectionSettings().ApiKey);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_api_key_fails(string apiKey)
        => Assert.Equal("An API key is required.", ExampleConnectionSettingsValidator.Validate(new ExampleConnectionSettings { ApiKey = apiKey }));

    [Fact]
    public void Missing_settings_fail()
        => Assert.Equal("An API key is required.", ExampleConnectionSettingsValidator.Validate(null));

    [Fact]
    public void Unresolved_reference_names_the_configuration_key()
    {
        // A reference arriving unresolved (Automate normally resolves it, or reports the missing key, first).
        var error = ExampleConnectionSettingsValidator.Validate(new ExampleConnectionSettings());

        Assert.NotNull(error);
        Assert.Contains("Umbraco:Automate:Secrets:Example:ApiKey", error);
    }

    [Fact]
    public void Real_key_passes()
        => Assert.Null(ExampleConnectionSettingsValidator.Validate(new ExampleConnectionSettings { ApiKey = "token" }));
}
