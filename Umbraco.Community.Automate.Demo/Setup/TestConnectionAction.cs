using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Demo.Setup;

/// <summary>
/// Demo only: runs a connection's own Test connection check from an automation, so each
/// "Test: ..." automation can tell the editor whether its connection works. Always succeeds, with
/// <see cref="TestConnectionOutput.Connected"/> for an If step to branch on, because a failed check
/// is the answer the editor asked for, not an error.
/// </summary>
[Action("demo.testConnection", "Test Connection",
    Description = "Runs a connection's Test connection check. Branch on its connected output with an If step.",
    Group = "Demo",
    Icon = "icon-link")]
public sealed class TestConnectionAction(ActionInfrastructure infrastructure, IConnectionService connectionService)
    : ActionBase<TestConnectionSettings, TestConnectionOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<TestConnectionSettings>();
        var connection = await connectionService.GetConnectionByAliasAsync(settings.ConnectionAlias, cancellationToken);
        if (connection is null)
            return Success(new TestConnectionOutput { Message = $"There's no connection with the alias '{settings.ConnectionAlias}'." });

        var result = await connectionService.TestConnectionAsync(connection.Id, cancellationToken);
        if (result is null)
            return Success(new TestConnectionOutput { Message = "The connection couldn't be tested." });

        // A warning still counts as connected (e.g. a live check that's switched off); the message says why.
        return Success(new TestConnectionOutput
        {
            Connected = result.Status != ConnectionValidationStatus.Failure,
            Message = result.Message ?? result.Status.ToString(),
        });
    }
}

public sealed class TestConnectionSettings
{
    [Field(Label = "Connection", Description = "The alias of the connection to test, e.g. pushover. Find it under Automation → Settings → Connections.", SortOrder = 10)]
    public string ConnectionAlias { get; set; } = string.Empty;
}

public sealed class TestConnectionOutput
{
    /// <summary>Whether the connection's check passed.</summary>
    public bool Connected { get; init; }

    /// <summary>What the connection's check reported, e.g. "Connected as @name" or why it failed.</summary>
    public string Message { get; init; } = string.Empty;
}
