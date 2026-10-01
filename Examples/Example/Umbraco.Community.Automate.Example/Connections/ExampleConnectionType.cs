using Umbraco.Automate.Core.Connections;
using Umbraco.Community.Automate.Example.Api;

namespace Umbraco.Community.Automate.Example.Connections;

/// <summary>
/// The Example connection: stores an API key and checks it when the user clicks
/// <b>Test connection</b>. The alias ("example") is stored in saved automations and set as each
/// action's ConnectionTypeAlias, so once released it must never change.
/// </summary>
[ConnectionType("example", "Example (httpbin.org)",
    Description = "A reference connection that talks to httpbin.org.",
    Group = "Example",
    Icon = "icon-automate-example")]
public sealed class ExampleConnectionType(ConnectionTypeInfrastructure infrastructure, ExampleClient client)
    : ConnectionTypeBase<ExampleConnectionSettings>(infrastructure)
{
    public override async Task<ConnectionValidationResult> ValidateAsync(object? settings, CancellationToken cancellationToken)
    {
        var typed = settings as ExampleConnectionSettings;

        // Check the settings first, without calling the service.
        if (ExampleConnectionSettingsValidator.Validate(typed) is { } error)
            return ConnectionValidationResult.Failure(error);

        // Then make one cheap authenticated call.
        try
        {
            var response = await client.CheckApiKeyAsync(typed!.ApiKey, cancellationToken);
            return response.Authenticated
                ? ConnectionValidationResult.Success("Connected to httpbin.org.")
                : ConnectionValidationResult.Failure("httpbin.org didn't accept the API key.");
        }
        catch (ExampleApiException ex)
        {
            return ConnectionValidationResult.Failure(ex.Message);
        }
    }
}
