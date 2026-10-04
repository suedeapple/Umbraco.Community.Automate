using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Demo.Setup;

/// <summary>
/// Demo only: runs a connection's own Test connection check from an automation, so each
/// "Test: ..." automation can tell the editor whether its connection works. Succeeds either way,
/// with an outcome to branch on, because a failed check is the answer the editor asked for.
/// </summary>
[Action("demo.testConnection", "Test Connection",
    Description = "Runs a connection's Test connection check and branches on the result.",
    Group = "Demo",
    Icon = "icon-plug")]
public sealed class TestConnectionAction(ActionInfrastructure infrastructure, IConnectionService connectionService)
    : ActionBase<TestConnectionSettings, TestConnectionOutput>(infrastructure)
{
    /// <summary>The check passed (or passed with a warning, which the message explains).</summary>
    public const string OutcomeConnected = "connected";

    /// <summary>The check failed, or there's no connection with that alias.</summary>
    public const string OutcomeFailed = "failed";

    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<TestConnectionSettings>();
        var connection = await connectionService.GetConnectionByAliasAsync(settings.ConnectionAlias, cancellationToken);
        if (connection is null)
            return SuccessWithOutcome(OutcomeFailed, new TestConnectionOutput { Message = $"There's no connection with the alias '{settings.ConnectionAlias}'." });

        var result = await connectionService.TestConnectionAsync(connection.Id, cancellationToken);
        if (result is null)
            return SuccessWithOutcome(OutcomeFailed, new TestConnectionOutput { Message = "The connection couldn't be tested." });

        var output = new TestConnectionOutput { Message = result.Message ?? result.Status.ToString() };

        return SuccessWithOutcome(result.Status == ConnectionValidationStatus.Failure ? OutcomeFailed : OutcomeConnected, output);
    }
}

public sealed class TestConnectionSettings
{
    [Field(Label = "Connection", Description = "The alias of the connection to test, e.g. pushover. Find it under Automation → Settings → Connections.", SortOrder = 10)]
    public string ConnectionAlias { get; set; } = string.Empty;
}

public sealed class TestConnectionOutput
{
    /// <summary>What the connection's check reported, e.g. "Connected as @name" or why it failed.</summary>
    public string Message { get; init; } = string.Empty;
}
