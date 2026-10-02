using Moq;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Testing;
using Umbraco.Community.Automate.Skoda.Actions;
using Umbraco.Community.Automate.Skoda.Connections;
using Xunit;
using Umbraco.Community.Automate.Skoda.Api;
using Umbraco.Community.Automate.Skoda.Models;

namespace Umbraco.Community.Automate.Skoda.Tests.Actions;

public class GetVehicleStatusActionTests
{
    private const string ApiKey = "test-api-key";
    private const string Vin = "TMBJB9NY5RF999999";

    [Fact]
    public async Task ExecuteAsync_maps_full_vehicle_response_to_output()
    {
        var vehicle = new Vehicle(
            Vin,
            "My Car",
            "AB123CD",
            new Uri("https://example.com"),
            null,
            null,
            new Odometer(12345, DateTimeOffset.UtcNow),
            new ParkingPosition("PARKED", new GpsCoordinates(52.1, 5.1), "Some Street 1, Prague"),
            new AirConditioning("COOLING", new TargetTemperature(21, "CELSIUS"), DateTimeOffset.UtcNow, false, false, new WindowHeating(false, "OFF", "OFF"), DateTimeOffset.UtcNow),
            null,
            null,
            new Charging(
                false,
                new ChargingStatus(10, 5, 30, DateTimeOffset.UtcNow, "CHARGING", "AC", new BatteryStatus(150000, 65)),
                new ChargingSettings(80, 80, "MANUAL", [], "DEACTIVATED", "PERMANENT", "MAXIMUM", 10),
                DateTimeOffset.UtcNow),
            null);

        var client = new Mock<ISkodaClient>();
        client.Setup(c => c.GetVehicleAsync(ApiKey, Vin, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new VehicleResponse(
                  vehicle,
                  [new VehicleError("SOME_ERROR", "Something could not be retrieved.")]));

        var result = await ActionTestHarness.For<GetVehicleStatusAction>()
            .WithService(client.Object)
            .WithSettings(new GetVehicleStatusSettings())
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Success, result.Status);
        var output = (GetVehicleStatusOutput)result.OutputData!;
        Assert.Equal(Vin, output.Vin);
        Assert.Equal("My Car", output.Name);
        Assert.Equal("AB123CD", output.LicensePlate);
        Assert.Equal(12345, output.MileageInKm);
        Assert.Equal(65, output.StateOfChargeInPercent);
        Assert.Equal(150000, output.RemainingCruisingRangeInMeters);
        Assert.Equal("CHARGING", output.ChargingState);
        Assert.Equal("COOLING", output.AirConditioningState);
        Assert.Equal("PARKED", output.ParkingState);
        Assert.Equal(52.1, output.Latitude);
        Assert.Equal(5.1, output.Longitude);
        Assert.Equal("Some Street 1, Prague", output.FormattedAddress);
        Assert.Single(output.Errors);
        Assert.Equal("SOME_ERROR", output.Errors.Single().Type);
        Assert.Equal("Something could not be retrieved.", output.Errors.Single().Description);
    }

    [Fact]
    public async Task ExecuteAsync_maps_unsupported_parts_to_null_instead_of_throwing()
    {
        var vehicle = new Vehicle(
            Vin,
            "My Car",
            "AB123CD",
            new Uri("https://example.com"),
            null, null, null, null, null, null, null, null, null);

        var client = new Mock<ISkodaClient>();
        client.Setup(c => c.GetVehicleAsync(ApiKey, Vin, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new VehicleResponse(vehicle, []));

        var result = await ActionTestHarness.For<GetVehicleStatusAction>()
            .WithService(client.Object)
            .WithSettings(new GetVehicleStatusSettings())
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Success, result.Status);
        var output = (GetVehicleStatusOutput)result.OutputData!;
        Assert.Null(output.MileageInKm);
        Assert.Null(output.StateOfChargeInPercent);
        Assert.Null(output.RemainingCruisingRangeInMeters);
        Assert.Null(output.ChargingState);
        Assert.Null(output.AirConditioningState);
        Assert.Null(output.ParkingState);
        Assert.Null(output.Latitude);
        Assert.Null(output.Longitude);
        Assert.Null(output.FormattedAddress);
        Assert.Empty(output.Errors);
    }
}
