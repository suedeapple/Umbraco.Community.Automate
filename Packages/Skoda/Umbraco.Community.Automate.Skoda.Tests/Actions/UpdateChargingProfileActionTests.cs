using Moq;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Testing;
using Umbraco.Community.Automate.Skoda.Actions;
using Umbraco.Community.Automate.Skoda.Connections;
using Xunit;
using Umbraco.Community.Automate.Skoda.Api;
using Umbraco.Community.Automate.Skoda.Models;
using Timer = Umbraco.Community.Automate.Skoda.Models.Timer;

namespace Umbraco.Community.Automate.Skoda.Tests.Actions;

public class UpdateChargingProfileActionTests
{
    private const string ApiKey = "test-api-key";
    private const string Vin = "TMBJB9NY5RF999999";

    [Fact]
    public async Task ExecuteAsync_overrides_only_the_fields_the_settings_specify()
    {
        var existingProfile = new ChargingProfile(
            123456,
            "Home",
            new ChargingProfileSettings("REDUCED", new MinBatteryStateOfCharge(true, 20), 80, "OFF"),
            [new ChargingTime(1, true, "22:00", "06:00")],
            [new Timer(1, true, "07:00", "CYCLIC", null, ["MONDAY"])]);

        var client = SetUpClientWithProfile(existingProfile);
        ChargingProfile? sentProfile = null;
        client.Setup(c => c.UpdateChargingProfileAsync(ApiKey, Vin, 123456, It.IsAny<ChargingProfile>(), It.IsAny<CancellationToken>()))
              .Callback<string, string, long, ChargingProfile, CancellationToken>((_, _, _, profile, _) => sentProfile = profile)
              .Returns(Task.CompletedTask);

        var result = await ActionTestHarness.For<UpdateChargingProfileAction>()
            .WithService(client.Object)
            .WithSettings(new UpdateChargingProfileSettings { ProfileId = 123456, TargetStateOfChargeInPercent = 90 })
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Success, result.Status);
        Assert.NotNull(sentProfile);
        Assert.Equal("Home", sentProfile.Name);
        Assert.Equal(90, sentProfile.Settings.TargetStateOfChargeInPercent);
        Assert.Equal("REDUCED", sentProfile.Settings.MaxChargingCurrent);
        Assert.Equal("OFF", sentProfile.Settings.AutoUnlockPlugWhenCharged);
        Assert.Equal(existingProfile.PreferredChargingTimes, sentProfile.PreferredChargingTimes);
        Assert.Equal(existingProfile.Timers, sentProfile.Timers);
    }

    [Fact]
    public async Task ExecuteAsync_overrides_all_exposed_fields_when_all_are_specified()
    {
        var existingProfile = new ChargingProfile(
            123456,
            "Home",
            new ChargingProfileSettings("REDUCED", new MinBatteryStateOfCharge(true, 20), 80, "OFF"),
            [],
            []);

        var client = SetUpClientWithProfile(existingProfile);
        ChargingProfile? sentProfile = null;
        client.Setup(c => c.UpdateChargingProfileAsync(ApiKey, Vin, 123456, It.IsAny<ChargingProfile>(), It.IsAny<CancellationToken>()))
              .Callback<string, string, long, ChargingProfile, CancellationToken>((_, _, _, profile, _) => sentProfile = profile)
              .Returns(Task.CompletedTask);

        var result = await ActionTestHarness.For<UpdateChargingProfileAction>()
            .WithService(client.Object)
            .WithSettings(new UpdateChargingProfileSettings
            {
                ProfileId = 123456,
                Name = "Work",
                TargetStateOfChargeInPercent = 100,
                MaxChargingCurrent = "MAXIMUM",
                AutoUnlockPlugWhenCharged = "PERMANENT",
            })
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Success, result.Status);
        Assert.Equal("Work", sentProfile!.Name);
        Assert.Equal(100, sentProfile.Settings.TargetStateOfChargeInPercent);
        Assert.Equal("MAXIMUM", sentProfile.Settings.MaxChargingCurrent);
        Assert.Equal("PERMANENT", sentProfile.Settings.AutoUnlockPlugWhenCharged);
    }

    [Fact]
    public async Task ExecuteAsync_fails_validation_without_calling_update_when_profile_id_is_not_found()
    {
        var client = SetUpClientWithProfile(new ChargingProfile(
            123456,
            "Home",
            new ChargingProfileSettings("REDUCED", new MinBatteryStateOfCharge(true, 20), 80, "OFF"),
            [],
            []));

        var result = await ActionTestHarness.For<UpdateChargingProfileAction>()
            .WithService(client.Object)
            .WithSettings(new UpdateChargingProfileSettings { ProfileId = 999999 })
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Failed, result.Status);
        Assert.Equal(StepRunErrorCategory.Validation, result.ErrorCategory);
        client.Verify(
            c => c.UpdateChargingProfileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<ChargingProfile>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_fails_validation_without_calling_update_when_vehicle_has_no_charging_profiles()
    {
        var vehicle = new Vehicle(Vin, "My Car", "AB123CD", new Uri("https://example.com"), null, null, null, null, null, null, null, null, null);
        var client = new Mock<ISkodaClient>();
        client.Setup(c => c.GetVehicleAsync(ApiKey, Vin, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new VehicleResponse(vehicle, []));

        var result = await ActionTestHarness.For<UpdateChargingProfileAction>()
            .WithService(client.Object)
            .WithSettings(new UpdateChargingProfileSettings { ProfileId = 123456 })
            .WithConnection("community.skoda", new SkodaConnectionSettings { ApiKey = ApiKey, Vin = Vin })
            .ExecuteAsync();

        Assert.Equal(ActionResultStatus.Failed, result.Status);
        Assert.Equal(StepRunErrorCategory.Validation, result.ErrorCategory);
        client.Verify(
            c => c.UpdateChargingProfileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<ChargingProfile>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<ISkodaClient> SetUpClientWithProfile(ChargingProfile profile)
    {
        var vehicle = new Vehicle(
            Vin, "My Car", "AB123CD", new Uri("https://example.com"),
            null, null, null, null, null, null, null, null,
            new ChargingProfiles([profile], null, DateTimeOffset.UtcNow));

        var client = new Mock<ISkodaClient>();
        client.Setup(c => c.GetVehicleAsync(ApiKey, Vin, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new VehicleResponse(vehicle, []));
        return client;
    }
}
