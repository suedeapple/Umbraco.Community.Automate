using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.DevTo.Actions;
using Umbraco.Community.Automate.DevTo.Articles;
using Umbraco.Community.Automate.DevTo.Content;
using Umbraco.Community.Automate.DevTo.Api;
using Umbraco.Community.Automate.DevTo.Connections;

namespace Umbraco.Community.Automate.DevTo.Composers;

public class DevToComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient(DevToClient.HttpClientName, client => client.Timeout = TimeSpan.FromSeconds(30));
        builder.Services.AddSingleton<DevToClient>();
        builder.Services.AddSingleton<DevToArticlePublisher>();
        builder.Services.AddSingleton<DevToArticleLinks>();
        builder.Services.AddScoped<DevToArticleChecker>();
        builder.Services.AddSingleton<ContentMarkdownConverter>();
        builder.Services.AddSingleton<DevToContentRenderer>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<PublishContentAction>();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<DevToConnectionType>();


    }
}
