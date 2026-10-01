using Umbraco.Automate.Core.Connections;
using Umbraco.Community.Automate.Skoda.Api;

namespace Umbraco.Community.Automate.Skoda.Connections;

[ConnectionType(
    SkodaConstants.ConnectionTypeAlias,
    "Škoda",
    Description = "Connects Umbraco Automate to a Škoda vehicle using the MyŠkoda Public API.",
    Icon = "icon-skoda")]
public sealed class SkodaConnectionType(ConnectionTypeInfrastructure infrastructure, ISkodaClient skodaClient) : ConnectionTypeBase<SkodaConnectionSettings>(infrastructure)
{
    public override async Task<ConnectionValidationResult> ValidateAsync(
    object? settings,
    CancellationToken cancellationToken)
    {
        if (settings is not SkodaConnectionSettings skodaSettings)
        {
            return ConnectionValidationResult.Failure("Invalid settings.");
        }

        if (string.IsNullOrWhiteSpace(skodaSettings.ApiKey) ||
            string.IsNullOrWhiteSpace(skodaSettings.Vin))
        {
            return ConnectionValidationResult.Failure("API key and VIN must be provided.");
        }

        // Automate replaces a $-reference with the configured value; one that's still here
        // means the key isn't in configuration.
        if (skodaSettings.ApiKey.TrimStart().StartsWith('$'))
        {
            return ConnectionValidationResult.Failure(
                $"The API key reference '{skodaSettings.ApiKey}' could not be resolved. Add the key to configuration at Umbraco:Community:Automate:Skoda:Secrets:ApiKey, or enter the key itself on the connection.");
        }

        if (!skodaSettings.ValidateConnection)
        {
            return ConnectionValidationResult.Warning("The connection was not validated against the Škoda API.");
        }

        try
        {
            await skodaClient.GetVehicleAsync(
                skodaSettings.ApiKey,
                skodaSettings.Vin,
                cancellationToken);

            return ConnectionValidationResult.Success();
        }
        catch (SkodaClientException ex)
        {
            return ConnectionValidationResult.Failure($"Unable to validate the Škoda connection: {ex.Message}");
        }
    }
}
