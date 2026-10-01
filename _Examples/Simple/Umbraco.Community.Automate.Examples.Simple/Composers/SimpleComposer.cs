using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.Examples.Simple.Actions;
using Umbraco.Community.Automate.Examples.Simple.Connections;

namespace Umbraco.Community.Automate.Examples.Simple.Composers;

/// <summary>Registers the connection type and action. Umbraco finds this composer on its own.</summary>
public class SimpleComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<SimpleConnectionType>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<SendMessageAction>();
    }
}
