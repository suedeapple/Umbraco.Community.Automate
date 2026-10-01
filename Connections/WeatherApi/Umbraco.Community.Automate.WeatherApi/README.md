# Umbraco.Community.Automate.WeatherApi

A [WeatherAPI.com](https://www.weatherapi.com/) connection type and actions for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate).

Get the current weather or today's forecast for a location as part of an automation workflow, for example to branch on the conditions, add the temperature to a notification, or log the weather alongside other workflow data.

## Installation

```bash
dotnet add package Umbraco.Community.Automate.WeatherApi
```

No further setup required. The composer registers itself automatically via Umbraco's `IComposer` discovery.

## Setup

### 1. Get an API key

Sign up at [weatherapi.com](https://www.weatherapi.com/) and copy the API key from your account dashboard. The free plan is enough for these actions.

### 2. Store the key in configuration

Add the key under `Umbraco:Automate:Secrets:WeatherApi`, in Umbraco Automate's shared Secrets section. Locally, use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) rather than `appsettings.json`:

```json
{
  "Umbraco": {
    "Automate": {
      "Secrets": {
        "WeatherApi": {
          "ApiKey": "your-api-key"
        }
      }
    }
  }
}
```

In production, use an environment variable instead:

```
Umbraco__Automate__Secrets__WeatherApi__ApiKey=your-api-key
```

Umbraco Automate resolves references from this shared section out of the box, so there's nothing to register.

### 3. Create the connection

1. Go to **Automation → Settings → Connections** and create a new **WeatherAPI.com** connection (in the **Weather** group).
2. **API Key** is already filled in with `$Umbraco:Automate:Secrets:WeatherApi:ApiKey`, a reference to the key you stored in step 2. Leave it as it is. If you'd rather keep the key on the connection itself, replace the reference with the key.
3. Click **Test connection**. This requests the current weather for London to confirm the key works.

## Actions

Both actions are in the **Weather** group and accept the same settings:

| Setting | Description |
|---|---|
| Location | Required. A city name, US zip, UK postcode, IP address, or `lat,lon`. Supports `${ binding }` expressions. |
| Culture | Optional. Localises the condition text, e.g. `en-GB`, `fr-FR`, `es-ES`. English if blank. Supports `${ binding }` expressions. |

### Get Current Weather

Gets the current conditions. Outputs, available to later steps as `${ steps.<alias>.<output> }`:

| Output | Description |
|---|---|
| `locationName`, `region`, `country`, `localTime` | The resolved location, e.g. "London", "United Kingdom". |
| `temperatureC`, `temperatureF` | The current temperature. |
| `condition`, `conditionCode`, `conditionIconUrl` | The condition text (e.g. "Sunny"), its WeatherAPI.com code (e.g. `1000`) and icon URL. |
| `humidity`, `cloud` | Humidity and cloud cover, as percentages. |
| `windKph`, `windMph`, `windDirection` | Wind speed and direction, e.g. "WSW". |
| `willItRain`, `chanceOfRain`, `willItSnow`, `chanceOfSnow` | Rain and snow expectations for the current hour. |
| `uv` | The UV index. |
| `lastUpdated` | When WeatherAPI.com last updated the data. |
| `rawResponse` | The full JSON response from WeatherAPI.com. |

### Get Today's Weather

Gets today's forecast. Outputs:

| Output | Description |
|---|---|
| `locationName`, `region`, `country`, `localTime` | The resolved location. |
| `date` | The forecast date, e.g. "2026-10-01". |
| `maxTemperatureC`, `maxTemperatureF`, `minTemperatureC`, `minTemperatureF`, `avgTemperatureC`, `avgTemperatureF` | Today's temperatures. |
| `maxWindKph`, `maxWindMph` | Today's maximum wind speed. |
| `totalPrecipMm`, `totalPrecipIn`, `totalSnowCm` | Today's total rain and snow. |
| `avgVisKm`, `avgVisMiles`, `avgHumidity` | Average visibility and humidity. |
| `willItRain`, `chanceOfRain`, `willItSnow`, `chanceOfSnow` | Rain and snow expectations for the day. |
| `condition`, `conditionCode`, `conditionIconUrl` | The day's condition, e.g. "Partly cloudy" (`1003`). |
| `uv` | The UV index. |
| `rawResponse` | The full JSON response from WeatherAPI.com. |

## Troubleshooting

**"Configuration key 'Umbraco:Automate:Secrets:WeatherApi:ApiKey' not found"**: the key isn't in configuration. Add it as in [step 2](#2-store-the-key-in-configuration), or replace the reference on the connection with the key itself.

**"API key is invalid."** or **"API key has been disabled."**: the key was rejected by WeatherAPI.com. Check it in your WeatherAPI.com dashboard.

**"No matching location found."**: WeatherAPI.com couldn't resolve the **Location**. Try a city name, postcode or `lat,lon`.

Rate limits, timeouts and WeatherAPI.com being unavailable are treated as temporary, so the step is retried according to its error behaviour. An invalid key, unknown location or missing setting fails straight away.

## Migrating from SA.Automate.WeatherApi

This package was previously published as `SA.Automate.WeatherApi` and now ships from the [Umbraco.Community.Automate](https://github.com/umbraco-community/Umbraco.Community.Automate) repo:

```bash
dotnet remove package SA.Automate.WeatherApi
dotnet add package Umbraco.Community.Automate.WeatherApi
```

Every connection in this repo uses a `community.` prefix, so the aliases changed:

| | Old | New |
|---|---|---|
| Connection type | `weatherApi` | `community.weatherApi` |
| Get Current Weather | `weatherApi.GetCurrentWeather` | `community.weatherApi.getCurrentWeather` |
| Get Today's Weather | `weatherApi.GetTodaysWeather` | `community.weatherApi.getTodaysWeather` |

Saved connections and automation steps refer to these aliases, so after upgrading:

1. Recreate your **WeatherAPI.com** connection. The API key is pre-filled with `$Umbraco:Automate:Secrets:WeatherApi:ApiKey`; move your key there (from `Umbraco:Automate:Secrets:WeatherApiKey`), or enter it on the connection.
2. In each automation that used a weather action, remove the old step, add it again with the new connection and the same settings, and publish the automation.

## Compatibility

| Package version | Umbraco Automate | Umbraco CMS |
|---|---|---|
| 1.x | 17.x – 18.x | 17.4 – 18.x |

## Links

- [Source code](https://github.com/umbraco-community/Umbraco.Community.Automate/tree/main/Connections/WeatherApi/Umbraco.Community.Automate.WeatherApi)
- [Report an issue](https://github.com/umbraco-community/Umbraco.Community.Automate/issues)
- [WeatherAPI.com documentation](https://www.weatherapi.com/docs/)
