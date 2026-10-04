using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Community.Automate.Demo.Setup;

/// <summary>
/// Creates an API user for Automate workspaces to run as. A workspace's Service Account Key only
/// accepts API users, and a fresh Demo database has just the Administrator, so without this
/// nobody could create a workspace until they'd made one by hand.
/// </summary>
public class DemoApiUserComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, DemoApiUserHandler>();
}

public class DemoApiUserHandler(
    IRuntimeState runtimeState,
    IUserService userService,
    ILogger<DemoApiUserHandler> logger) : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    // Umbraco requires usernames to be email addresses by default.
    private const string UserName = "automate-demo@example.com";

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        // Only once Umbraco is installed; on the first run, the unattended install finishes first.
        if (runtimeState.Level != RuntimeLevel.Run || userService.GetByUsername(UserName) is not null)
            return;

        var result = await userService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Kind = UserKind.Api,
                Name = "Automate (Demo)",
                UserName = UserName,
                Email = UserName,
                UserGroupKeys = new HashSet<Guid> { Constants.Security.AdminGroupKey },
            },
            approveUser: true);

        if (result.Success)
            logger.LogInformation("Created the {Name} API user for Automate workspaces", "Automate (Demo)");
        else
            logger.LogWarning("Couldn't create the Demo's API user for Automate workspaces: {Status}", result.Status);
    }
}
