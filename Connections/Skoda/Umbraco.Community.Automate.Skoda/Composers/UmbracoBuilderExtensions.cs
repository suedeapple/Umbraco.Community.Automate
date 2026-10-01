using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Automate.Core.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.Skoda.Configuration;
using Umbraco.Community.Automate.Skoda.Api;

namespace Umbraco.Community.Automate.Skoda.Composers;

internal static class UmbracoBuilderExtensions
{
    extension(IUmbracoBuilder builder)
    {
        public IUmbracoBuilder AddSkodaAutomate()
        {
            builder.Services
               .AddOptions<SkodaOptions>()
               .Bind(builder.Config.GetSection(SkodaOptions.SectionName));

            builder.Services.AddHttpClient<ISkodaClient, SkodaClient>(
                (serviceProvider, client) =>
                {
                    var options = serviceProvider
                        .GetRequiredService<IOptions<SkodaOptions>>()
                        .Value;

                    client.BaseAddress = options.BaseUrl;
                });

            // Automate only resolves $-references under allow-listed prefixes, so the default
            // $Umbraco:Community:Automate:Skoda:Secrets:ApiKey reference needs this.
            builder.Services.PostConfigure<AutomateOptions>(options =>
            {
                options.AllowedConfigurationKeyPrefixes =
                [
                    .. options.AllowedConfigurationKeyPrefixes,
                    SkodaConfiguration.VariablesPath,
                    SkodaConfiguration.SecretsPath,
                ];
                options.SecretConfigurationKeyPrefixes =
                [
                    .. options.SecretConfigurationKeyPrefixes,
                    SkodaConfiguration.SecretsPath,
                ];
            });

            return builder;
        }
    }
}
