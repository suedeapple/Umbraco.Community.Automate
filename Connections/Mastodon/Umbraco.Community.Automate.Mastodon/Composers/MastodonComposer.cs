using Microsoft.Extensions.DependencyInjection;
using Umbraco.Community.Automate.Mastodon.Actions;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Configuration;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Community.Automate.Mastodon.Api;
using Umbraco.Community.Automate.Mastodon.Configuration;
using Umbraco.Community.Automate.Mastodon.Connections;

namespace Umbraco.Community.Automate.Mastodon.Composers;

public class MastodonComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<MastodonClientFactory>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<SendMastodonPostAction>();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<MastodonConnectionType>();

        builder.Services.AddSingleton<IPackageManifestReader, MastodonPackageManifestReader>();

        // Automate only resolves $-references under allow-listed prefixes
        builder.Services.PostConfigure<AutomateOptions>(options =>
        {
            options.AllowedConfigurationKeyPrefixes =
            [
                .. options.AllowedConfigurationKeyPrefixes,
                MastodonConfiguration.VariablesPath,
                MastodonConfiguration.SecretsPath,
            ];
            options.SecretConfigurationKeyPrefixes =
            [
                .. options.SecretConfigurationKeyPrefixes,
                MastodonConfiguration.SecretsPath,
            ];
        });
    }
}
