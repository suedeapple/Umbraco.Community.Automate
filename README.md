# Umbraco.Community.Automate

Community-built packages for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate): connection types, actions and triggers that let your automations talk to services Umbraco doesn't cover out of the box.

Umbraco Automate adds an **Automation** section to the Umbraco backoffice where you build workflows: a *trigger* (for example "content published") followed by a series of *actions* (for example "append a row to a Google Sheet", "post to Mastodon"). Each package in this repo adds one service. Install the package, configure its credentials, and its actions show up in the workflow builder.

## Packages

| Package | What it does | Status |
|---|---|---|
| [DevTo](Packages/DevTo/Umbraco.Community.Automate.DevTo/README.md) | Cross-posts Umbraco content to [DEV Community](https://dev.to). Converts Markdown, Rich Text, Block List and Block Grid to Markdown, and sets the canonical URL to your site. | [![NuGet](https://img.shields.io/nuget/vpre/Umbraco.Community.Automate.DevTo?color=0273B3)](https://www.nuget.org/packages/Umbraco.Community.Automate.DevTo) pre-release |
| [GoogleSheets](Packages/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/README.md) | Connects to Google Sheets with OAuth. Appends, finds, updates, deletes and upserts rows; reads and clears ranges; creates spreadsheets and tabs. | Not yet on NuGet: [build from source](#using-a-package-before-its-on-nuget) |
| [Mastodon](Packages/Mastodon/Umbraco.Community.Automate.Mastodon/README.md) | Posts statuses to any Mastodon instance, with visibility, content warnings and an appended URL. Replaces `OC.Automate.Mastodon`. | Not yet on NuGet: [build from source](#using-a-package-before-its-on-nuget) |
| [Pushover](Packages/Pushover/Umbraco.Community.Automate.Pushover/README.md) | Sends push notifications to phones and desktops through [Pushover](https://pushover.net/), with titles, links, sounds and priorities, including emergency alerts that repeat until acknowledged. Replaces `SA.Automate.Pushover`. | Not yet on NuGet: [build from source](#using-a-package-before-its-on-nuget) |
| [Skoda](Packages/Skoda/Umbraco.Community.Automate.Skoda/README.md) | Reads a Škoda vehicle's status and controls charging, air conditioning, auxiliary heating and ventilation through the MyŠkoda Public API. | In development, not yet on NuGet |
| [WeatherApi](Packages/WeatherApi/Umbraco.Community.Automate.WeatherApi/README.md) | Gets the current weather or today's forecast for a location from [WeatherAPI.com](https://www.weatherapi.com/). Replaces `SA.Automate.WeatherApi`. | Not yet on NuGet under this name: [build from source](#using-a-package-before-its-on-nuget) |

Each package's README has the full setup guide, all its settings, and troubleshooting.

### Examples

`Packages/_Examples/` holds two small, working reference connections to copy when you start a new one. Both talk to [httpbin.org](https://httpbin.org), so they work in the Demo site with no signup, and both are built and tested by CI but never published.

- [**Simple**](Packages/_Examples/Simple/Umbraco.Community.Automate.Examples.Simple/README.md): an API key and one action, the shape most connections start with.
- [**Kitchen Sink**](Packages/_Examples/KitchenSink/Umbraco.Community.Automate.Examples.KitchenSink/README.md): every convention in one place: a connection, two actions, a trigger, outcomes, an API client, a backoffice front end and tests.

## Requirements

- Umbraco CMS 17 (DevTo, Mastodon and WeatherApi also support 18; see each package's *Compatibility* section)
- [Umbraco Automate](https://www.nuget.org/packages/Umbraco.Automate) 17 installed in the site
- .NET 10

## Getting started

These steps are the same for every package. The package READMEs fill in the service-specific parts.

### 1. Install the package

```bash
dotnet add package Umbraco.Community.Automate.DevTo --prerelease
```

There is nothing to register in `Program.cs`. Each package registers itself through Umbraco's composer discovery.

### 2. Add credentials to configuration

Each package reads its settings from configuration, so secrets stay out of the database and out of source control. Most need just an API key. For example, WeatherApi:

```json
{
  "Umbraco": {
    "Automate": {
      "Secrets": {
        "WeatherApi": { "ApiKey": "your-api-key" }
      }
    }
  }
}
```

Every connection keeps its values under its own name inside Umbraco Automate's shared `Variables` (non-sensitive) and `Secrets` (sensitive) sections, which Automate reads references from out of the box, so there's nothing to register. Each package's README lists its keys.

Locally, use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) rather than `appsettings.json`. In production, use environment variables (`Umbraco__Automate__Secrets__WeatherApi__ApiKey=...`) or a key vault. Google Sheets uses OAuth instead: you supply a Client ID and Secret under `Umbraco:Automate:Providers:GoogleSheets`, then sign in from the backoffice.

### 3. Create a connection

In the backoffice, open **Automation → Settings → Connections** and create a connection of the package's type. Credential fields come pre-filled with a reference to configuration, e.g. `$Umbraco:Automate:Secrets:WeatherApi:ApiKey`, so values you stored in step 2 are used without typing anything. Replace a reference with the value itself if you'd rather store it on the connection. Click **Test connection** to check it; if a key is missing from configuration, Umbraco Automate tells you which one (*Configuration key '...' not found*).

### 4. Build an automation

Under **Automation → Automations**, open a workspace and create an automation:

1. Pick a **trigger**, e.g. *Content Published*, limited to your blog post document type.
2. Add one or more **actions** from the package, e.g. *Send Mastodon Post*, and choose the connection you created.
3. Use `${ ... }` bindings to pass data between steps, e.g. `${ trigger.contentName }` or `${ steps.getContent.properties.summary }`.

Publish a matching page, then check **Automation → Runs** to see what happened at each step.

### Using a package before it's on NuGet

Until a package is published, reference it from a local clone of this repo:

```bash
git clone https://github.com/umbraco-community/Umbraco.Community.Automate.git
dotnet add <your-site>.csproj reference Umbraco.Community.Automate/Packages/Mastodon/Umbraco.Community.Automate.Mastodon/Umbraco.Community.Automate.Mastodon.csproj
```

Or pack it into a local feed:

```bash
dotnet pack Packages/Mastodon/Umbraco.Community.Automate.Mastodon -c Release -o ./local-feed
dotnet add <your-site>.csproj package Umbraco.Community.Automate.Mastodon --source ./local-feed --prerelease
```

For Google Sheets, build its backoffice front end first (`npm ci && npm run build` in `Packages/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/Client`). Without that step its column-list editor won't load.

## Trying everything in the Demo site

`Umbraco.Community.Automate.Demo/` is a disposable Umbraco site that references every package. It installs itself into a local SQLite database the first time it runs, so you don't need a database server or any setup screens.

```bash
# Backoffice front ends are generated, not committed: build them once
cd Packages/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/Client
npm ci && npm run build
cd ../../../..
cd Packages/_Examples/KitchenSink/Umbraco.Community.Automate.Examples.KitchenSink/Client
npm ci && npm run build
cd ../../../../..

dotnet run --project Umbraco.Community.Automate.Demo
```

Open <https://localhost:44343/umbraco> and log in as `admin@example.com` / `password1234`. The **Automation** section is in the top navigation.

[CONTRIBUTING.md](.github/CONTRIBUTING.md#running-the-demo-site) covers using real credentials, resetting the site, and running the E2E tests against it.

## Contributing

Bug reports, fixes and new packages are all welcome. [CONTRIBUTING.md](.github/CONTRIBUTING.md) covers the repo layout, building, testing, the Demo site, adding a new package and how releases work.

Found a problem, or have an idea for a package? [Open an issue](https://github.com/umbraco-community/Umbraco.Community.Automate/issues/new/choose). For a security problem, please follow [SECURITY.md](.github/SECURITY.md) and report it privately instead.

## License

MIT. See [LICENSE](LICENSE) for details.
