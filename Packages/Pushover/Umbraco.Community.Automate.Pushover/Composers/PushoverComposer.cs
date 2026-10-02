using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.Pushover.Actions;
using Umbraco.Community.Automate.Pushover.Api;
using Umbraco.Community.Automate.Pushover.Connections;

namespace Umbraco.Community.Automate.Pushover.Composers;

/// <summary>Registers everything the package adds. Umbraco finds this composer on its own.</summary>
public class PushoverComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<PushoverClient>();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<PushoverConnectionType>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<SendNotificationAction>();

        // No icon registration: Umbraco finds wwwroot/umbraco-package.json on its own.
        // No configuration registration: the default references live under Automate's shared
        // Umbraco:Automate:Secrets section, which it resolves out of the box.
    }
}
