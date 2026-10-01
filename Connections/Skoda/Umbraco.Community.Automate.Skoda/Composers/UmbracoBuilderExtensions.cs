using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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


            return builder;
        }
    }
}
