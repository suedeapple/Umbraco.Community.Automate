using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Skoda.Connections;
using Umbraco.Community.Automate.Skoda.Api;

namespace Umbraco.Community.Automate.Skoda.Actions;

[Action(
    "community.skoda.stopActiveVentilation", 
    "Stop Active Ventilation", 
    Group = "Skoda", 
    Icon = "icon-automate-skoda", 
    ConnectionTypeAlias = "community.skoda")]
public sealed class StopActiveVentilationAction(ActionInfrastructure infrastructure, ISkodaClient client) : ActionBase<StopActiveVentilationSettings, VehicleCommandOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<StopActiveVentilationSettings>();
        var connection = context.Connection ?? throw new InvalidOperationException("A Škoda connection is required.");
        var connectionSettings = connection.GetSettings<SkodaConnectionSettings>();

        await client.StopActiveVentilationAsync(connectionSettings.ApiKey, connectionSettings.Vin, cancellationToken);

        return Success(new VehicleCommandOutput { Vin = connectionSettings.Vin });
    }
}
