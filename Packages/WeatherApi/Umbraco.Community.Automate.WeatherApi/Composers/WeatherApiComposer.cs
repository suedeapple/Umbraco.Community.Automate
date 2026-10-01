using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.WeatherApi.Actions;
using Umbraco.Community.Automate.WeatherApi.Connections;

namespace Umbraco.Community.Automate.WeatherApi.Composers;

/// <summary>
/// Registers the WeatherAPI.com connection type and actions with Umbraco Automate.
/// </summary>
public class WeatherApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<WeatherApiConnectionType>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<GetCurrentWeatherAction>()
            .Add<GetTodaysWeatherAction>();

    }
}
