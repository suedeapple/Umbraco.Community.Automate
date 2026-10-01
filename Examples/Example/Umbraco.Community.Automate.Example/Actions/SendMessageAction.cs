using Microsoft.Extensions.Logging;
using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Example.Api;
using Umbraco.Community.Automate.Example.Connections;
using Umbraco.Community.Automate.Example.Models;

namespace Umbraco.Community.Automate.Example.Actions;

/// <summary>
/// Sends a message to the service and returns what it received: the shape of most "create
/// something" actions (post to a channel, add a row, publish an article).
/// </summary>
[Action("community.example.sendMessage", "Send Example Message",
    Description = "Sends a message to httpbin.org and returns what it received.",
    ConnectionTypeAlias = "community.example",
    Group = "Example",
    Icon = "icon-automate-example")]
public sealed class SendMessageAction(
    ActionInfrastructure infrastructure,
    ExampleClient client,
    ILogger<SendMessageAction> logger)
    : ActionBase<SendMessageSettings, SendMessageOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        // 1. Validate the connection and settings first, failing fast with a message the user can act on.
        var connection = context.Connection?.GetSettings<ExampleConnectionSettings>();
        if (ExampleConnectionSettingsValidator.Validate(connection) is { } connectionError)
            return ActionResult.Failed(new InvalidOperationException(connectionError), StepRunErrorCategory.ConfigurationError);

        var settings = context.GetSettings<SendMessageSettings>();
        if (string.IsNullOrWhiteSpace(settings.Message))
            return ActionResult.Failed(new InvalidOperationException("A message is required."), StepRunErrorCategory.Validation);

        // 2. Call the service. The idempotency key is the same on a retry of this step, so the
        //    service can ignore the duplicate instead of posting twice.
        try
        {
            var response = await client.SendMessageAsync(
                connection!.ApiKey,
                new SendMessageRequest { Message = settings.Message },
                idempotencyKey: $"{context.RunId}:{context.StepId}",
                cancellationToken);

            // Structured placeholders, and never the API key.
            logger.LogInformation("Run {RunId}: sent a {Length}-character Example message", context.RunId, settings.Message.Length);

            // 3. Return a typed output for later steps.
            return Success(new SendMessageOutput { Message = response.Json?.Message ?? string.Empty, Url = response.Url });
        }
        catch (ExampleApiException ex)
        {
            // The client already chose the category, which decides whether Automate retries.
            return ActionResult.Failed(ex, ex.Category);
        }
    }
}
