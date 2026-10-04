using System.Reflection;
using System.Text.Json;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Settings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
#if USYNC
using Umbraco.Automate.Core.Workspaces;
using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.SyncHandlers.Models;
#endif

namespace Umbraco.Community.Automate.Demo.Setup;

/// <summary>
/// Gets a Demo database ready to use on startup, so a fresh one (or a reset) needs no setup:
/// <list type="number">
///   <item>an API user for workspaces to run as (a workspace's Service Account Key only accepts API users);</item>
///   <item>one connection per package in the repo, pre-filled with its configuration references;</item>
///   <item>the sample workspace and automations in uSync/, imported once these exist (Umbraco 17 only,
///   as uSync.Automate has no 18 release yet).</item>
/// </list>
/// Each step only adds what's missing, so it never touches anything you've changed.
/// </summary>
public class DemoSetupComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, DemoSetupHandler>();
}

public class DemoSetupHandler(
    IRuntimeState runtimeState,
    IUserService userService,
    ConnectionTypeCollection connectionTypes,
    IConnectionService connectionService,
#if USYNC
    IWorkspaceService workspaceService,
    ISyncService syncService,
    ISyncConfigService syncConfig,
#endif
    ILogger<DemoSetupHandler> logger) : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    /// <summary>Fixed, so the sample workspace in uSync/ can name it as its service account.</summary>
    public static readonly Guid ApiUserKey = new("4fc20353-4d4f-4798-b936-bcb18008e152");

    private const string ApiUserName = "Automate API User";

    // Umbraco requires usernames to be email addresses by default.
    private const string ApiUserLogin = "automate-demo@example.com";

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        // Only once Umbraco is installed; on the first run, the unattended install finishes first.
        if (runtimeState.Level != RuntimeLevel.Run)
            return;

        await EnsureApiUserAsync();
        await EnsureConnectionsAsync(cancellationToken);
#if USYNC
        await ImportSamplesAsync(cancellationToken);
#endif
    }

    private async Task EnsureApiUserAsync()
    {
        if (userService.GetByUsername(ApiUserLogin) is not null)
            return;

        var result = await userService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Id = ApiUserKey,
                Kind = UserKind.Api,
                Name = ApiUserName,
                UserName = ApiUserLogin,
                Email = ApiUserLogin,
                UserGroupKeys = new HashSet<Guid> { Constants.Security.AdminGroupKey },
            },
            approveUser: true);

        if (result.Success)
            logger.LogInformation("Created the {Name} for Automate workspaces", ApiUserName);
        else
            logger.LogWarning("Couldn't create the Demo's API user for Automate workspaces: {Status}", result.Status);
    }

    /// <summary>
    /// One connection for each of the repo's connection types, aliased after the type (e.g.
    /// community.weatherApi becomes weatherApi) so the sample automations can find it. Its settings
    /// are the settings class's defaults: the configuration references the package pre-fills, which
    /// resolve to the placeholders in appsettings.Development.json, or your appsettings.Local.json.
    /// </summary>
    private async Task EnsureConnectionsAsync(CancellationToken cancellationToken)
    {
        var existing = (await connectionService.GetAllConnectionsAsync(cancellationToken))
            .Select(c => c.Alias).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var type in connectionTypes.Where(t => t.Alias.StartsWith("community.", StringComparison.Ordinal)))
        {
            var alias = type.Alias["community.".Length..].Replace('.', '-');
            if (existing.Contains(alias))
                continue;

            await connectionService.CreateConnectionAsync(
                new Connection { Alias = alias, Name = type.Name, Type = type.Alias, Settings = type.SettingsType is { } settingsType ? DefaultSettings(settingsType) : [] },
                Constants.Security.SuperUserKey,
                cancellationToken);
            logger.LogInformation("Created the {Name} connection ({Alias})", type.Name, alias);
        }
    }

    private static Dictionary<string, object?> DefaultSettings(Type settingsType)
    {
        var defaults = Activator.CreateInstance(settingsType);
        var settings = new Dictionary<string, object?>();
        foreach (var property in settingsType.GetProperties().Where(p => p.GetCustomAttribute<EditableModelFieldAttribute>() is not null))
        {
            var value = property.GetValue(defaults);
            var unset = value is null or "" || (property.PropertyType.IsValueType && value.Equals(Activator.CreateInstance(property.PropertyType)));
            if (!unset)
                settings[JsonNamingPolicy.CamelCase.ConvertName(property.Name)] = value;
        }

        return settings;
    }

#if USYNC
    /// <summary>
    /// Imports the sample workspace and automations from uSync/ into a database that has no
    /// workspaces yet. Done here rather than with uSync's own ImportOnFirstBoot, because they refer
    /// to the API user and connections above, which have to exist first.
    /// </summary>
    private async Task ImportSamplesAsync(CancellationToken cancellationToken)
    {
        if ((await workspaceService.GetAllWorkspacesAsync(cancellationToken)).Any())
            return;

        var result = await syncService.StartupImportAsync(
            syncConfig.GetFolders(), false, new SyncHandlerOptions { Group = "Automate" }, null);
        logger.LogInformation("Imported the sample workspace and automations: {Count} item(s)", result.Count());
    }
#endif
}
