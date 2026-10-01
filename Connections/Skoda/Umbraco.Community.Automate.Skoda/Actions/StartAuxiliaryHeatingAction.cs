using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Skoda.Connections;
using Umbraco.Community.Automate.Skoda.Api;
using Umbraco.Community.Automate.Skoda.Models;

namespace Umbraco.Community.Automate.Skoda.Actions;

[Action(
    "community.skoda.startAuxiliaryHeating",
    "Start Auxiliary Heating",
    Group = "Skoda",
    Icon = "icon-skoda",
    ConnectionTypeAlias = "community.skoda")]
public sealed class StartAuxiliaryHeatingAction(ActionInfrastructure infrastructure, ISkodaClient client) : ActionBase<StartAuxiliaryHeatingSettings, VehicleCommandOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<StartAuxiliaryHeatingSettings>();
        var connection = context.Connection ?? throw new InvalidOperationException("A Škoda connection is required.");
        var connectionSettings = connection.GetSettings<SkodaConnectionSettings>();
        if (string.IsNullOrWhiteSpace(settings.Spin))
            return ActionResult.Failed(new ArgumentException("S-PIN is required."));

        var configuration = new StartAuxiliaryHeatingConfiguration(
            new TargetTemperature(settings.TargetTemperature, settings.TemperatureUnit),
            settings.Spin,
            settings.DurationInSeconds,
            settings.StartMode);

        await client.StartAuxiliaryHeatingAsync(
            connectionSettings.ApiKey,
            connectionSettings.Vin,
            configuration,
            cancellationToken);

        return Success(new VehicleCommandOutput { Vin = connectionSettings.Vin });
    }
}
