using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Skoda.Client;
using Umbraco.Community.Automate.Skoda.Connections;

namespace Umbraco.Community.Automate.Skoda.Actions;

[Action(
    "community.automate.skoda.setChargeMode",
    "Set Charge Mode",
    Group = "Skoda",
    Icon = "icon-skoda",
    ConnectionTypeAlias = SkodaConstants.ConnectionTypeAlias)]
public sealed class SetChargeModeAction(ActionInfrastructure infrastructure, ISkodaClient client) : ActionBase<SetChargeModeSettings, VehicleCommandOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<SetChargeModeSettings>();
        var connection = context.Connection ?? throw new InvalidOperationException("A Škoda connection is required.");
        var connectionSettings = connection.GetSettings<SkodaConnectionSettings>();

        await client.SetChargeModeAsync(
            connectionSettings.ApiKey,
            connectionSettings.Vin,
            settings.ChargeMode,
            cancellationToken);

        return Success(new VehicleCommandOutput { Vin = connectionSettings.Vin });
    }
}
