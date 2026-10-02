using Moq;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Testing;
using Umbraco.Community.Automate.Skoda.Actions;
using Umbraco.Community.Automate.Skoda.Connections;
using Xunit;
using Umbraco.Community.Automate.Skoda.Api;
using Umbraco.Community.Automate.Skoda.Models;

namespace Umbraco.Community.Automate.Skoda.Tests.Actions;

public class StartAuxiliaryHeatingActionTests
{
    private const string ApiKey = "test-api-key";
    private const string Vin = "TMBJB9NY5RF999999";

    [Fact]
    public async Task ExecuteAsync_builds_configuration_from_settings()
    {
        var client = new Mock<ISkodaClient>();
        StartAuxiliaryHeatingConfiguration? sentConfiguration = null;
        client.Setup(c => c.StartAuxiliaryHeatingAsync(ApiKey, Vin, It.IsAny<StartAuxiliaryHeatingConfiguration>(), It.IsAny<CancellationToken>()))
              .Callback<string, string, StartAuxiliaryHeatingConfiguration, CancellationToken>((_, _, config, _) => sentConfiguration = config)
              .Returns(Task.CompletedTask);

        var result = await ActionTestHarness.For<StartAuxiliaryHeatingAction>()
            .WithService(client.Object)
            .WithSettings(new StartAuxiliaryHeatingSettings
            {
                TargetTemperature = 23,
                TemperatureUnit = "CELSIUS",
                Spin = "1234",
                DurationInSeconds = 900,
                StartMode = "VENTILATION",
            })
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Success, result.Status);
        Assert.NotNull(sentConfiguration);
        Assert.Equal("1234", sentConfiguration.Spin);
        Assert.Equal(900, sentConfiguration.DurationInSeconds);
        Assert.Equal("VENTILATION", sentConfiguration.StartMode);
        Assert.Equal(23, sentConfiguration.TargetTemperature.Value);
        Assert.Equal(Vin, ((VehicleCommandOutput)result.OutputData!).Vin);
    }

    [Fact]
    public async Task ExecuteAsync_fails_validation_without_calling_the_api_when_spin_is_missing()
    {
        var client = new Mock<ISkodaClient>();

        var result = await ActionTestHarness.For<StartAuxiliaryHeatingAction>()
            .WithService(client.Object)
            .WithSettings(new StartAuxiliaryHeatingSettings { Spin = "" })
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Failed, result.Status);
        client.Verify(
            c => c.StartAuxiliaryHeatingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StartAuxiliaryHeatingConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
