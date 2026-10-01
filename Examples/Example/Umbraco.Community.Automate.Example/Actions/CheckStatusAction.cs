using System.Net;
using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Example.Api;
using Umbraco.Community.Automate.Example.Connections;

namespace Umbraco.Community.Automate.Example.Actions;

/// <summary>
/// Shows outcomes: instead of failing when something isn't there, the action succeeds with an
/// outcome that later steps can branch on, like a "find" or "look up" action would.
/// </summary>
[Action("community.example.checkStatus", "Check Example Status",
    Description = "Asks httpbin.org for a status code and branches on the answer.",
    ConnectionTypeAlias = "community.example",
    Group = "Example",
    Icon = "icon-automate-example")]
public sealed class CheckStatusAction(ActionInfrastructure infrastructure, ExampleClient client)
    : ActionBase<CheckStatusSettings, CheckStatusOutput>(infrastructure)
{
    /// <summary>The service answered with a success status.</summary>
    public const string OutcomeAvailable = "available";

    /// <summary>The service answered 404: a normal result to branch on, not an error.</summary>
    public const string OutcomeNotFound = "notFound";

    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var connection = context.Connection?.GetSettings<ExampleConnectionSettings>();
        if (ExampleConnectionSettingsValidator.Validate(connection) is { } connectionError)
            return ActionResult.Failed(new InvalidOperationException(connectionError), StepRunErrorCategory.ConfigurationError);

        var settings = context.GetSettings<CheckStatusSettings>();
        if (settings.StatusCode is < 100 or > 599)
            return ActionResult.Failed(new InvalidOperationException("Status code must be between 100 and 599."), StepRunErrorCategory.Validation);

        try
        {
            var status = await client.GetStatusAsync(connection!.ApiKey, settings.StatusCode, cancellationToken);
            var output = new CheckStatusOutput { StatusCode = (int)status };

            if ((int)status is >= 200 and < 300)
                return SuccessWithOutcome(OutcomeAvailable, output);

            if (status == HttpStatusCode.NotFound)
                return SuccessWithOutcome(OutcomeNotFound, output);

            // Anything else is a real failure, classified so Automate knows whether to retry.
            return ActionResult.Failed(
                new InvalidOperationException($"httpbin.org answered {(int)status}."),
                ExampleClient.Classify(status));
        }
        catch (ExampleApiException ex)
        {
            return ActionResult.Failed(ex, ex.Category);
        }
    }
}
