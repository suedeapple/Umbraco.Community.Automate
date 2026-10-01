---
name: automate-connection
description: Scaffold and build Umbraco Automate connection packages in the Umbraco.Community.Automate repo the house way: the Connections/<Area>/ layout, standard folder names (Actions, Triggers, Connections, Composers, Api, Configuration, Client), project wiring (solution, Demo site, Directory.Build.props, MinVer, CI), tests, README and secrets handling. Use this whenever someone wants to create a new connection or provider (e.g. "add a Slack connection", "build a Bluesky integration for Automate"), add an action or trigger to an existing connection, check a connection follows the repo conventions, or asks where a class or file should go in this repo, even if they don't say "connection" or "skill".
---

# Umbraco Automate connections

This repo is a monorepo of community connections for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate). Each connection is one NuGet package that adds a **connection type** (credentials for an external service) and **actions** (and later **triggers**) that automations can use.

Every connection follows the same shape on purpose: contributors and reviewers should be able to open any package and know where everything is, and CI discovers packages purely from the folder layout. Follow this skill so a new connection looks like the existing ones. When in doubt, copy **`Examples/Example/`**: a small reference connection (talking to httpbin.org) that uses every folder here, including a trigger, outcomes, a custom field editor in `Client/`, and xUnit-only tests. It's built and tested by CI but never published. Among the real connections, **WeatherApi** is the simplest; **Google Sheets** shows OAuth; **DevTo** shows content conversion in depth.

Code templates for every file below are in [references/templates.md](references/templates.md). Read it before writing code.

## Layout

```
Connections/<Area>/                               <Area> = PascalCase service name, e.g. Bluesky
  Umbraco.Community.Automate.<Area>/              the package
    Actions/        one class per action + its settings and output classes
    Triggers/       one class per trigger + its settings and output classes (when needed)
    Connections/    the connection type + its connection settings (+ validator)
    Composers/      IComposer, package manifest reader, IUmbracoBuilder extensions
    Api/            C# client for the external service: client, error mapping, exceptions
    Models/         request and response models for the external API
    Configuration/  <Area>Configuration (config paths + default references), plus options classes if genuinely needed
    Client/         backoffice front end (Vite + Lit, npm). Only if custom UI is needed
    wwwroot/        static backoffice files served under App_Plugins/ (icons)
    Directory.Build.props
    README.md
    community-automate-128.png
    Umbraco.Community.Automate.<Area>.csproj
  Umbraco.Community.Automate.<Area>.Tests/        xUnit tests, same folder names where grouped
```

Why these names:
- **Plural** for Automate's building blocks (`Actions`, `Triggers`, `Connections`) and for `Composers`, matching each other and Umbraco's own convention for composers.
- **`Connections/`, not `Connection/` or `ConnectionType/`**: a singular `Connection` namespace shadows Automate's `Connection` type inside the package's own namespace tree and breaks code that uses it.
- **`Client/` is only ever the npm front end.** CI gives a package front-end jobs when `Client/package.json` exists, so a C# API client must go in `Api/`, never `Client/`.
- Namespaces follow folders: `Umbraco.Community.Automate.<Area>.Actions`, `.Connections`, and so on.
- Folders unique to one connection (e.g. DevTo's `Articles/`, `Content/`) are fine alongside the standard ones.

Don't create empty folders: add `Triggers/`, `Api/`, `Models/`, `Configuration/`, `Client/` or `wwwroot/` only when there's something to put in them.

Keep the structure flat: these packages are small, so standard folders sit directly in the package root (models in a root `Models/`, not `Api/Models/`), which makes everything easy to find.

## Creating a new connection

Work through these in order and tick them off. Ask the user for the service name, what it should do (which actions), and how it authenticates (API key/token, OAuth, none) before starting.

1. **Projects.** Create the package and `.Tests` projects under `Connections/<Area>/` from the templates. If the package has a `wwwroot/` (custom icons), use `Microsoft.NET.Sdk.Razor` with `StaticWebAssetBasePath` `App_Plugins/UmbracoCommunityAutomate<Area>` so it's served as static web assets; otherwise plain `Microsoft.NET.Sdk` is enough (see WeatherApi).
2. **`Directory.Build.props`** in the package folder, with `MinVerTagPrefix` `<area-lowercase>-v` and `MinVerIgnoreHeight` true. This file is also the marker that makes CI find the package; without it the package is silently skipped.
3. **Configuration class** `Configuration/<Area>Configuration.cs` holding the config paths and each credential's default reference (see Configuration and secrets).
4. **Connection type and settings** in `Connections/`, with credential fields defaulting to those references.
5. **Actions** in `Actions/`, each with its settings (and output, if it returns data).
6. **Composer** in `Composers/`: register the connection type, actions, any services and the package manifest (icons). Configuration references need no registration (see Configuration below).
7. **Icons**: either a built-in Umbraco icon (e.g. `icon-partly-cloudy`, no files needed) or a custom one in `wwwroot/` plus the manifest reader in `Composers/`.
8. **Package versions** go in the root `Directory.Packages.props` only; `.csproj` files never specify versions.
9. **Wire it in**: add both projects to `Umbraco.Community.Automate.slnx` (in a `/Connections/<Area>/` solution folder) and a `ProjectReference` from `Demo/Umbraco.Community.Automate.Demo.csproj`.
10. **Demo placeholders**: if the Demo site needs settings to boot, add obviously fake values (e.g. `"e2e-test"`) to `Demo/appsettings.Development.json`. Real credentials go in user secrets only.
11. **Tests** for every action and the connection validation (see Testing).
12. **README.md** in the package (see README), and a row in the root `README.md` connections table.
13. **Verify**: `dotnet build`, `dotnet test`, `dotnet pack Connections/<Area>/Umbraco.Community.Automate.<Area> -c Release -o ./pack-check`, then run the Demo site: check the connection type appears under **Automation → Settings → Connections → Create** with its icon and pre-filled references, and that **Test connection** resolves them (put placeholder values in `Demo/appsettings.Development.json` or user secrets).

You don't need to edit any CI workflow: `ci.yml` discovers packages from the layout, and `release.yml` finds a package from its tag prefix.

## Conventions

### Aliases, names and groups
- **Aliases are permanent.** Saved automations reference the connection type alias and action aliases, so never rename an existing one. Pick them carefully up front.
- For new connections use `<area>` (camelCase) for the connection type alias and `<area>.<actionName>` for actions, e.g. `bluesky` and `bluesky.createPost`. (Existing packages vary; leave them as they are.)
- Set `ConnectionTypeAlias` on every action to the connection type alias, so the action only offers matching connections.
- Write the alias, name, group and icon directly on each `[ConnectionType]`, `[Action]` and `[Trigger]` attribute, as Umbraco Automate's own step types do. Don't gather them into an `<Area>Constants` class: the attribute is where a reader looks for them.
- Use a shared `Group` where one fits (`Social Networks`, `Productivity`) so related connections sit together in the pickers.
- Use the same icon on the connection type and its actions. A built-in Umbraco icon is fine; name custom icons `icon-automate-<area>`.

### Settings fields
- Use `[Field(Label = ..., Description = ..., SortOrder = ...)]` on every setting; the description is the user's only help text in the backoffice.
- Mark credentials `IsSensitive = true`.
- Set `SupportsBindings = true` on action settings that should accept `${ ... }` values from the trigger or earlier steps (content, titles, IDs).
- Put rarely-changed options in `Group = "Advanced"`.

### Configuration and secrets
- Never commit real credentials, not even temporarily. The repo's gitleaks hooks (`.githooks/`) block commits and pushes that contain them.
- Credentials live in configuration, not the database, under Umbraco Automate's **shared** sections, nested under the area name: `Umbraco:Automate:Secrets:<Area>:<Key>` for sensitive values and `Umbraco:Automate:Variables:<Area>:<Key>` for the rest, referenced from fields as `$Umbraco:Automate:Secrets:<Area>:ApiKey`. Automate resolves references from these two sections by default (its allow-list is default-deny for everything else), so **don't register anything** in the composer. Nesting under `<Area>` keeps a package's keys together and avoids clashes. Secrets can only be referenced from `IsSensitive` fields. Keep the paths, and each default reference, as constants in `Configuration/<Area>Configuration.cs`, and use them everywhere the path appears (settings defaults, validator messages), so there's a single place to change them.
- **Pre-fill each credential field with its reference as the default value** (`public string ApiKey { get; set; } = <Area>Configuration.ApiKeyReference;`). A new connection then opens with the reference already filled in, so users who store the key in configuration don't type anything, and anyone who prefers can overwrite it with the value. Don't ask users to copy and paste references from help text, and don't repeat the reference path in the field's description either: the pre-filled value already shows it, so the description only needs to say what the value is and where to get it. Leave per-connection values (like a vehicle VIN) blank.
- Automate resolves references before settings reach your code. If the key is missing it fails the step or the **Test connection** itself, with *Configuration key '...' not found*, so you don't need to handle that. As a cheap safety net, the shared validator can still reject a value that starts with `$` (one that arrived unresolved) with a message naming the key, before any API call.
- Don't add configuration for values that don't vary between sites, such as the service's API base URL: make it a constant on the API client (e.g. `private const string BaseUrl` in `Api/<Area>Client.cs`, or `internal` if the composer or tests need it). Skoda had a `BaseUrl` options class nobody needed, and it was removed. If a package genuinely needs an options class, bind it to the same `Umbraco:Automate:Variables:<Area>` section (`<Area>Configuration.VariablesPath`) so all its configuration lives together.
- The one exception: OAuth providers use `Umbraco:Automate:Providers:<Area>`, which is where Umbraco Automate's OAuth support expects them (see Google Sheets).

### Actions
- Return `Success()` or `Success(output)` on success. Use `SuccessWithOutcome("found", output)`-style outcomes when later steps should branch (e.g. `created`/`updated`, `found`/`notFound`), and document each outcome.
- Return a typed output (`ActionBase<TSettings, TOutput>`) when the action produces data later steps need. Output properties are exposed in camelCase as `${ steps.<alias>.<property> }`.
- Fail with `ActionResult.Failed(exception, StepRunErrorCategory.X)` and pick the category deliberately, because Automate uses it to decide whether to retry: `Validation` and `ConfigurationError` for problems the user must fix, `Authentication` for 401/403, `RateLimiting` for 429, `Timeout`, `ServiceUnavailable` for 5xx, `InvalidResponse` for anything else unexpected. Map HTTP status codes in one place in `Api/` (DevTo's `Classify` method is the model).
- Validate the connection and settings first and fail fast with a clear message telling the user what to fix.
- Send an idempotency key where the service supports one (`$"{context.RunId}:{context.StepId}"`), so a retried step doesn't post twice.
- Treat `TaskCanceledException` when the caller didn't cancel as a timeout, and `HttpRequestException` as the service being unreachable.
- Log with structured placeholders, never secrets.

### Triggers
- The simplest trigger is a **notification trigger**: derive from `NotificationTriggerBase<TSettings, TOutput, TNotification>` (note the order: settings, output, then the Umbraco notification) and put `[Trigger("<area>.<name>", "Display Name", Group = ..., Icon = ...)]` on it. See `Examples/Example/Triggers/`.
- Override `MapEvent(notification)` to return one `TriggerEvent<TOutput>` per affected item, with `TriggerAlias = Alias`, the output, and `IdempotencyKey = GenerateIdempotencyKey(item.Key, 0, item.UpdateDate)` so a duplicate notification doesn't run the automation twice.
- Override `CanHandle(output, settings)` (it's `protected`, and `settings` can be null) to apply each automation's own filters, such as a key prefix.
- Automate subscribes to the notification for you; just register the trigger with `TriggerCollectionBuilder` in the composer. Output properties reach steps in camelCase as `${ trigger.<property> }`.
- Other kinds exist (`ScheduledTriggerBase` for CRON, `WebhookTriggerBase`, or raising events yourself with `ITriggerDispatcher`), but Automate supplies their output itself; reach for them only when a notification trigger can't do the job.

### Connection validation
- `ValidateAsync` should check required fields, then make one cheap authenticated call and return `Success("Connected as @name")`-style feedback.
- If each check costs the user API quota, make the live check opt-in (see Skoda's `ValidateConnection`).

## Testing

- **xUnit only**: use xUnit's `Assert`, and don't add assertion or mocking libraries (no Shouldly, FluentAssertions, Moq or NSubstitute). One framework keeps tests readable for every contributor and the dependency list short. Some older packages still use Shouldly or Moq; don't copy that into new code.
- Umbraco's own `Umbraco.Automate.Testing` package is fine (it's the official harness, not a mocking library): `ActionTestHarness.For<TAction>().WithService(...).WithSettings(...).WithConnection(alias, settings).ExecuteAsync()` runs an action end to end and returns its `ActionResult`.
- Instead of a mocking library, write small hand-written fakes: a stub `HttpMessageHandler` that returns canned responses and records requests, behind a tiny `IHttpClientFactory` implementation (see the templates).
- To unit test a connection type's `ValidateAsync`, construct it with `new ConnectionTypeInfrastructure(fakeResolver)`, where the fake implements `IEditableModelResolver` with explicit interface members that throw (validation never uses it). Explicit implementation avoids having to repeat the interface's generic constraint.
- Test each action's success path, each outcome, missing or invalid settings (including an unresolved `$` reference), and how API errors map to categories. Never call the real service.
- Group tests in the same folders as the package (`Actions/`, `Connections/`, `Api/`).
- If the connection has a `Client/` front end, add web-test-runner unit tests, and Playwright specs if it needs end-to-end coverage (see Google Sheets).

## README

The package README ships inside the NuGet package and is what nuget.org and the Umbraco Marketplace show, so write it for site developers installing the package. Sections, in order: one-line summary; **Installation** (`dotnet add package`); **Setup** (getting credentials from the service; storing them under `Umbraco:Automate:Secrets:<Area>` / `Umbraco:Automate:Variables:<Area>` with an appsettings example and the environment-variable equivalents, e.g. `Umbraco__Automate__Secrets__<Area>__ApiKey`; creating the connection under **Automation → Settings → Connections** and noting the fields come pre-filled with references); **Actions** (a table of every setting); **Outcomes and outputs**; **Troubleshooting** (the error messages users will actually see, including Automate's *Configuration key '...' not found* for a missing key); **Compatibility**; **Links**. Use absolute URLs for anything outside the package folder, because relative links break on nuget.org.

## Adding to an existing connection

- **New action**: add it to `Actions/` with its settings and output, register it in the composer, add tests, and document it in the package README. One action per PR is the norm.
- **New trigger**: same, in `Triggers/`.
- Keep existing aliases, namespaces and public types stable. They're used by people's saved automations and code.

## Importing an existing package

To bring in a connection that already lives in its own repo (as WeatherApi did from `SA.Automate.WeatherApi`):

- Work from the source repo's latest `origin/main`, not a possibly stale local checkout.
- Move the files into the standard folders and rename namespaces to `Umbraco.Community.Automate.<Area>.*`; the package ID becomes `Umbraco.Community.Automate.<Area>`.
- **Keep every alias exactly as it is** (connection type and actions), so existing connections and automations keep working after the switch.
- Replace hard-coded package versions with central ones, add `Directory.Build.props`, MinVer and tests, and drop files this repo handles centrally (release workflow, `umbraco-marketplace.json`, `NuGet.config`, separate licence/icon copies).
- Apply the conventions here (configuration class, pre-filled references under the shared sections, error categories, validators) and note any behaviour change in the README. If the old package used a different config path (WeatherApi used `Umbraco:Automate:Secrets:WeatherApiKey`), say that existing connections keep their saved values and only new ones get the new default.
- Add a **Migrating from <old package>** README section with the `dotnet remove`/`dotnet add` commands and a note that aliases are unchanged (see the Mastodon and WeatherApi READMEs).

## Working notes

This skill is a living document. Add new conventions and lessons learned here (or in the sections above) as the team agrees on them.

- Front-end packages pin Playwright to the version Google Sheets uses (`"overrides": { "playwright": "1.61.0", "playwright-core": "1.61.0" }` in `Client/package.json`), so every `Client/` shares one downloaded browser.
- Registering icons and editors: a package with a `Client/` lists them in `Client/src/manifests.ts`, loaded by `Client/public/umbraco-package.json` (Example, Google Sheets); one with only hand-written icons uses an `IPackageManifestReader` (Mastodon, DevTo; see the template). Skoda's static `wwwroot/umbraco-package.json` also works but isn't the pattern to copy.
- Automate also discovers `[ConnectionType]` and `[Action]` classes on its own: Skoda's composer registers neither, and both still appear. Register them explicitly in the composer anyway, so a reader can see everything the package adds in one place.
- Config references went through a few designs before settling: per-package `Umbraco:Community:Automate:<Area>` sections needed allow-list registration in every composer, so they moved under Automate's shared sections (nested by area, no registration); and the paths live in one `<Area>Configuration` class rather than being hard-coded, so there's one place to change them.
- .NET accepts any well-formed culture name (e.g. `xx-YY`), so `CultureInfo.GetCultureInfo` only throws for malformed input. Don't rely on it to reject unknown cultures.
