using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.Examples.KitchenSink.Actions;
using Umbraco.Community.Automate.Examples.KitchenSink.Api;
using Umbraco.Community.Automate.Examples.KitchenSink.Connections;
using Umbraco.Community.Automate.Examples.KitchenSink.Triggers;

namespace Umbraco.Community.Automate.Examples.KitchenSink.Composers;

/// <summary>
/// Registers everything the package adds. Umbraco finds this composer on its own, so sites
/// only install the package. Automate can also discover attributed types itself, but listing
/// them here shows everything the package adds in one place.
/// </summary>
public class KitchenSinkComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<KitchenSinkClient>();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<KitchenSinkConnectionType>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<SendMessageAction>()
            .Add<CheckStatusAction>();

        builder.WithCollectionBuilder<TriggerCollectionBuilder>()
            .Add<DictionaryItemSavedTrigger>();

        // No configuration registration: the default API key reference lives under Automate's
        // shared Umbraco:Automate:Secrets section, which it resolves out of the box. Icons and
        // the message editor are registered by Client/public/umbraco-package.json.
    }
}
