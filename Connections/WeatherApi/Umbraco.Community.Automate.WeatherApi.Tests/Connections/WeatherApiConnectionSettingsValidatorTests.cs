using Umbraco.Community.Automate.WeatherApi.Connections;
using Xunit;

namespace Umbraco.Community.Automate.WeatherApi.Tests.Connections;

public class WeatherApiConnectionSettingsValidatorTests
{
    [Fact]
    public void New_connections_default_to_the_configuration_reference()
        => Assert.Equal("$Umbraco:Community:Automate:WeatherApi:Secrets:ApiKey", new WeatherApiConnectionSettings().ApiKey);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_api_key_fails(string apiKey)
        => Assert.Equal("A WeatherAPI.com API key is required.",
            WeatherApiConnectionSettingsValidator.Validate(new WeatherApiConnectionSettings { ApiKey = apiKey }));

    [Fact]
    public void Missing_settings_fail()
        => Assert.Equal("A WeatherAPI.com API key is required.", WeatherApiConnectionSettingsValidator.Validate(null));

    [Fact]
    public void Unresolved_default_reference_explains_where_to_put_the_key()
    {
        // Automate leaves the reference as-is when the key isn't in configuration.
        var error = WeatherApiConnectionSettingsValidator.Validate(new WeatherApiConnectionSettings());

        Assert.NotNull(error);
        Assert.Contains("could not be resolved", error);
        Assert.Contains("Umbraco:Community:Automate:WeatherApi:Secrets:ApiKey", error);
    }

    [Fact]
    public void Real_key_passes()
        => Assert.Null(WeatherApiConnectionSettingsValidator.Validate(new WeatherApiConnectionSettings { ApiKey = "abc123" }));
}
