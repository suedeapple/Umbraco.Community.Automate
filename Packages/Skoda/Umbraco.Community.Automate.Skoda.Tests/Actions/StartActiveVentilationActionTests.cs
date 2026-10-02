using Moq;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Testing;
using Umbraco.Community.Automate.Skoda.Actions;
using Umbraco.Community.Automate.Skoda.Connections;
using Xunit;
using Umbraco.Community.Automate.Skoda.Api;

namespace Umbraco.Community.Automate.Skoda.Tests.Actions;

public class StartActiveVentilationActionTests
{
    private const string ApiKey = "test-api-key";
    private const string Vin = "TMBJB9NY5RF999999";

    [Fact]
    public async Task ExecuteAsync_starts_active_ventilation_for_the_connections_vehicle()
    {
        var client = new Mock<ISkodaClient>();

        var result = await ActionTestHarness.For<StartActiveVentilationAction>()
            .WithService(client.Object)
            .WithSettings(new StartActiveVentilationSettings())
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Success, result.Status);
        client.Verify(c => c.StartActiveVentilationAsync(ApiKey, Vin, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(Vin, ((VehicleCommandOutput)result.OutputData!).Vin);
    }
}
