---
name: umbraco-automate
description: The house rules for building anything for Umbraco Automate in the Umbraco.Community.Automate repo: connections, actions, triggers, outcomes and outputs, API clients, configuration and secrets ($-references in Umbraco:Automate:Secrets/Variables), backoffice front ends (Client/, Vite + Lit), tests (xUnit only), the Demo site, CI and releases. Covers the Packages/<Area>/ layout, standard folder names (Actions, Triggers, Connections, Composers, Api, Models, Configuration, Client), community.<area> aliases and project wiring, with Packages/_Examples/Simple and Packages/_Examples/KitchenSink as the references to copy. Use it whenever someone wants to create or change a package, connection, action or trigger (e.g. "build me an Automate package for Facebook", "add a Slack connection", "add a trigger to Mastodon"), migrate or import an existing Automate package into the repo or bring one up to the current conventions, write tests or a front end for one, wire something into the Demo site, or asks where something goes or how something is done in this repo, even if they don't mention Automate or the skill.
---

# Building for Umbraco Automate

This repo is a monorepo of community packages for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate). Each connection is one NuGet package that adds a **connection type** (credentials for an external service), **actions** and **triggers** that automations can use. This skill covers anything built here: new connections, new actions or triggers on existing ones, front ends, configuration, tests and the Demo site.

Every connection follows the same shape on purpose: contributors and reviewers should be able to open any package and know where everything is, and CI discovers packages purely from the folder layout. Follow this skill so a new connection looks like the existing ones. When in doubt, copy one of the two reference connections in `Packages/_Examples/` (both talk to httpbin.org, are built and tested by CI, and are never published): **`Packages/_Examples/Simple/`** is an API key and one action, the shape most connections need; **`Packages/_Examples/KitchenSink/`** uses every folder here, including a trigger, outcomes, an API client, a custom field editor in `Client/`, and xUnit-only tests. Among the real connections, **WeatherApi** is the simplest; **Google Sheets** shows OAuth; **DevTo** shows content conversion in depth.

## Start here

Work out which kind of request this is, because each has its own process:

- **A new package** ("build me a package for Facebook", "I want a Slack integration"): don't start coding. Follow [references/new-package.md](references/new-package.md): research the service, then take the user through multiple-choice decisions (what it does, how it connects, shape, names, settings) and build only after they confirm a summary.
- **Migrating an existing package into this repo**, or **bringing a package here up to the current conventions**: follow [references/migrating.md](references/migrating.md), which reads the source, asks about anything that affects existing users (aliases, configuration, package ID), then reshapes it.
- **A change to an existing package** (a new action, a fix, a new setting): no interview needed. Follow "Adding to an existing connection" below and the conventions.
- **A question** about where something goes or how something is done: answer it from this skill and the repo.

When you do need the user to choose, ask multiple-choice questions with a recommended option first (the `AskUserQuestion` tool when available), rather than open questions or silent assumptions.

## Choose a shape first

Most community connections are simple: an API key and an action or two. Build that unless the package needs more, and grow it later; never add folders "just in case".

| | **Simple** (most packages) | **Full** (only what's needed from it) |
|---|---|---|
| Use when | an API key (or token) and actions that each make one call | a trigger, outcomes, several actions sharing request and error handling, a custom icon, or a custom field editor |
| Folders | `Actions/`, `Composers/`, `Configuration/`, `Connections/` | any of the standard folders below, including `Api/`, `Models/`, `Triggers/`, `Client/`, `wwwroot/` |
| Icon | a built-in Umbraco icon, no files | built-in, or custom files in `umbraco-package.json` + `icons/` |
| Reference code | `Packages/_Examples/Simple/` | `Packages/_Examples/KitchenSink/` |
| Every file, ready to copy | [references/simple.md](references/simple.md) | [references/full.md](references/full.md) |

Read the matching reference before writing code, and [references/fields.md](references/fields.md) for every settings class. Both are complete (project files, every class, tests and wiring) and are checked by generating a package from them and running its tests. The guided process in [references/new-package.md](references/new-package.md) settles the shape with the user: recommend Simple unless their answers need something from the Full column. When a Simple package later needs one Full feature, add just that section from `full.md`: the files are written to fit together.

## Layout

```
Packages/<Area>/                                  <Area> = PascalCase name, usually the service, e.g. Bluesky
  Umbraco.Community.Automate.<Area>/              the package
    Actions/        one class per action + its settings and output classes
    Triggers/       one class per trigger + its settings and output classes (when needed)
    Connections/    the connection type + its connection settings (+ validator)
    Composers/      IComposer, IUmbracoBuilder extensions
    Api/            C# client for the external service: client, error mapping, exceptions
    Models/         request and response models for the external API
    Configuration/  <Area>Configuration (config paths, default references, fixed service values), plus options classes if genuinely needed
    Client/         backoffice front end (Vite + Lit, npm). Only if custom UI is needed
    wwwroot/        static backoffice files served under App_Plugins/: umbraco-package.json, icons/
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

A package doesn't have to be a connection. One that only adds a trigger or an action (for example an Umbraco-side trigger, or an action that needs no external service) is just as welcome: it uses the same layout with only the folders it needs, say `Triggers/` and `Composers/`, and no `Connections/`. See **Packages without a provider** below.

Keep the structure flat: these packages are small, so standard folders sit directly in the package root (models in a root `Models/`, not `Api/Models/`), which makes everything easy to find.

## Creating a new connection

Work through these in order and tick them off. For a package with no connection, skip the connection steps (3, 4 and the connection validation) and the `ConnectionTypeAlias` on actions; everything else applies. Before starting, settle the service, its actions, authentication, shape and names with the user through the guided process in [references/new-package.md](references/new-package.md).

1. **Projects.** Create the package and `.Tests` projects under `Packages/<Area>/` from [simple.md](references/simple.md) or [full.md](references/full.md). If the package has a `wwwroot/` (custom icons), use `Microsoft.NET.Sdk.Razor` with `StaticWebAssetBasePath` `App_Plugins/UmbracoCommunityAutomate<Area>` so it's served as static web assets; otherwise plain `Microsoft.NET.Sdk` is enough (see WeatherApi).
2. **`Directory.Build.props`** in the package folder, with `MinVerTagPrefix` `<area-lowercase>-v` and `MinVerIgnoreHeight` true. This file is also the marker that makes CI find the package; without it the package is silently skipped.
3. **Configuration class** `Configuration/<Area>Configuration.cs` holding the config paths, each credential's default reference and the service's fixed values such as its base URL (see Configuration and secrets).
4. **Connection type and settings** in `Connections/`, with credential fields defaulting to those references.
5. **Actions** in `Actions/`, each with its settings (and output, if it returns data). Full: an `Api/` client shared by the actions, outcomes, and **triggers** in `Triggers/`.
6. **Composer** in `Composers/`: register the package's **services** only (HTTP clients, an API client). Don't register the connection type, actions or triggers: Automate discovers them from their `[ConnectionType]`, `[Action]` and `[Trigger]` attributes. Icons and configuration references need no registration either.
7. **Icons**: either a built-in Umbraco icon (e.g. `icon-partly-cloudy`, no files needed) or a custom one, always registered the same way: `wwwroot/umbraco-package.json` lists `icons/icons.js`, which lists one `icons/<area>.icon.js` per icon (in `Client/public/` instead if the package has a `Client/`). No C# is involved; add the icon test from [full.md](references/full.md#tests).
8. **Package versions** go in the root `Directory.Packages.props` only; `.csproj` files never specify versions.
9. **Wire it in**: add both projects to `Umbraco.Community.Automate.Demo.slnx` (in a `/Packages/<Area>/` solution folder) and a `ProjectReference` from `Umbraco.Community.Automate.Demo/Umbraco.Community.Automate.Demo.csproj`. Add the package to the list in `.github/ISSUE_TEMPLATE/bug-report.yml`, and a line for it with its maintainers in `.github/CODEOWNERS`.
10. **Demo configuration**: add **every** configuration key the package reads (each credential reference, any variables, OAuth provider settings) to the Demo site, in two files, under the same paths as `<Area>Configuration`:
    - `Umbraco.Community.Automate.Demo/appsettings.Development.json`: obviously fake placeholders (`"e2e-test"`), so the site boots, new connections' references resolve, and CI's end-to-end runs work without real credentials.
    - `Umbraco.Community.Automate.Demo/appsettings.Local.example.json`: the same keys with `your-...` values, so a contributor copies it to the git-ignored `appsettings.Local.json` and fills in real credentials to try the package live. Use non-empty values: an empty one overrides the placeholder and can stop the site starting.
    Real credentials never go in either tracked file; they go in `appsettings.Local.json` or user secrets.
11. **Tests** for every action, outcome and trigger and the connection validation (see Testing). Full with a `Client/`: front-end tests too, and build it (`npm ci && npm run build`) before running the Demo site or packing.
12. **README.md** in the package (see README), and a row in the root `README.md` connections table.
13. **Verify**: `dotnet build`, `dotnet test`, `dotnet pack Packages/<Area>/Umbraco.Community.Automate.<Area> -c Release -o ./pack-check`, then run the Demo site: check the connection type appears under **Automation → Settings → Connections → Create** with its icon and pre-filled references, and that **Test connection** resolves them (put placeholder values in `Umbraco.Community.Automate.Demo/appsettings.Development.json` or user secrets).

You don't need to edit any CI workflow: `ci.yml` discovers packages from the layout, and `release.yml` finds a package from its tag prefix.

## Conventions

### Aliases, names and groups
- **Aliases are permanent once released.** Saved connections and automations store the connection type, action and trigger aliases, so renaming one breaks them. Pick them carefully up front.
- Prefix every alias with `community.`: `community.<area>` for the connection type and `community.<area>.<name>` (camelCase) for actions and triggers, e.g. `community.bluesky` and `community.bluesky.createPost`. The prefix keeps community packages apart from Umbraco's own step types. Every new package uses it, with the `Umbraco.Community.Automate.<Area>` package ID and namespaces. The only exception is a migrated package whose author chooses to keep their own package ID, namespaces and aliases (see [references/migrating.md](references/migrating.md)); the rest of the conventions still apply to it.
- Set `ConnectionTypeAlias` on every action to the connection type alias, so the action only offers matching connections.
- Write the alias, name, group and icon directly on each `[ConnectionType]`, `[Action]` and `[Trigger]` attribute, as Umbraco Automate's own step types do. Don't gather them into an `<Area>Constants` class: the attribute is where a reader looks for them.
- Use a shared `Group` so related connections sit together in the pickers, picking from the ones already in use before inventing one: **Social Networks** (Mastodon, DevTo), **Productivity** (Google Sheets), **Notifications** (Pushover), **Weather** (WeatherApi), **Vehicles** (Skoda). Name a new group after the kind of service, never the service itself, so the next similar package can join it. The group is only a display heading, not stored in automations, so it can change later.
- Use the same icon on the connection type, its actions and triggers. A built-in Umbraco icon is fine; name custom icons `icon-automate-<area>`.
- Register backoffice extensions (icons, editors, modals) in a static `umbraco-package.json`, which Umbraco discovers under `App_Plugins/` by itself. Don't use an `IPackageManifestReader` or list icons in `Client/src/`: one pattern everywhere means a contributor adding an icon only ever touches the same three files.

### Settings fields
Pick the right editor for every setting rather than leaving everything a text box: **read [references/fields.md](references/fields.md) whenever you write a settings class.** It lists Umbraco's editors, their `EditorConfig`, the C# type each binds to, and examples. In short:
- Use `[Field(Label = ..., Description = ..., SortOrder = ...)]` on every setting; the description is the user's only help text, so say what the value is, its range or format, and the default.
- **Could the value come from the trigger or an earlier step?** Keep it text with `SupportsBindings = true` (titles, messages, IDs, URLs). Bindings are text, so they can't go in a dropdown, toggle or number input.
- **One of a fixed set?** `Umb.PropertyEditorUi.Dropdown`, or `RadioButtonList` for two to five options, bound to `string` (or `CheckBoxList` and `List<string>` for several). Use `{ "name": "Friendly label", "value": "STORED_VALUE" }` items when the service's values aren't readable; the stored value can never change once released.
- **Longer text?** `Umb.PropertyEditorUi.TextArea` with `rows`. **A number?** `Integer` or `Decimal` with `min`/`max`, plus `[Range]`. `bool` gets a toggle and `DateTime` a date picker automatically. Media folders and items use `MediaPicker`; content, media type and member group filters use the type pickers.
- Give every option a default in the property initializer, so a new step works without changes.
- Use data annotations (`[Required]`, `[Range]`, `[StringLength]`) for single-field rules: Automate checks them before `ExecuteAsync` and fails the step as `Validation`.
- Every field needs a `Label` and a `Description`: without them the backoffice shows a raw `#uaFields_...` localization key. A `Group` other than `Advanced` needs a localization file for its heading (see DevTo), so prefer `Advanced`.
- Mark credentials `IsSensitive = true`, and put rarely-changed options in `Group = "Advanced"`.
- Build a custom editor in `Client/` only when no built-in editor can express the value (Google Sheets' column list is the model).

### Configuration and secrets
- Never commit real credentials, not even temporarily. The repo's gitleaks hooks (`.githooks/`) block commits and pushes that contain them.
- Credentials live in configuration, not the database, under Umbraco Automate's **shared** sections, nested under the area name: `Umbraco:Automate:Secrets:<Area>:<Key>` for sensitive values and `Umbraco:Automate:Variables:<Area>:<Key>` for the rest, referenced from fields as `$Umbraco:Automate:Secrets:<Area>:ApiKey`. Automate resolves references from these two sections by default (its allow-list is default-deny for everything else), so **don't register anything** in the composer. Nesting under `<Area>` keeps a package's keys together and avoids clashes. Secrets can only be referenced from `IsSensitive` fields. Keep the paths, and each default reference, as constants in `Configuration/<Area>Configuration.cs`, and use them everywhere the path appears (settings defaults, validator messages), so there's a single place to change them.
- **Pre-fill each credential field with its reference as the default value** (`public string ApiKey { get; set; } = <Area>Configuration.ApiKeyReference;`). A new connection then opens with the reference already filled in, so users who store the key in configuration don't type anything, and anyone who prefers can overwrite it with the value. Don't ask users to copy and paste references from help text, and don't repeat the reference path in the field's description either: the pre-filled value already shows it, so the description only needs to say what the value is and where to get it. Leave per-connection values (like a vehicle VIN) blank.
- Automate resolves references before settings reach your code. If the key is missing it fails the step or the **Test connection** itself, with *Configuration key '...' not found*, so you don't need to handle that. As a cheap safety net, the shared validator can still reject a value that starts with `$` (one that arrived unresolved) with a message naming the key, before any API call.
- Values that are the same for every site, such as the service's API base URL, a default instance URL (DevTo) or an OAuth scope (Google Sheets), are constants in `<Area>Configuration` too, not settings in `appsettings.json`: nobody needs to change them per site, but keeping them in that class means every fixed fact about the service is in one place, and a value used in several files (Google Sheets' API URL, used by every action) is defined once. Implementation details such as page sizes and timeouts stay private in the class that uses them, and a field's default value (e.g. `= 80`) stays on the field. If a package genuinely needs an options class, bind it to the same `Umbraco:Automate:Variables:<Area>` section (`<Area>Configuration.VariablesPath`) so all its configuration lives together.
- The one exception: OAuth providers use `Umbraco:Automate:Providers:<Area>`, which is where Umbraco Automate's OAuth support expects them (see Google Sheets).

### Actions
- Return `Success()` or `Success(output)` on success. Use `SuccessWithOutcome("found", output)`-style outcomes when later steps should branch (e.g. `created`/`updated`, `found`/`notFound`), and document each outcome.
- Return a typed output (`ActionBase<TSettings, TOutput>`) when the action produces data later steps need. Output properties are exposed in camelCase as `${ steps.<alias>.<property> }`.
- Fail with `ActionResult.Failed(exception, StepRunErrorCategory.X)` and pick the category deliberately, because Automate uses it to decide whether to retry: `Validation` and `ConfigurationError` for problems the user must fix, `Authentication` for 401/403, `RateLimiting` for 429, `Timeout`, `ServiceUnavailable` for 5xx, `InvalidResponse` for anything else unexpected. Map HTTP status codes in one place in `Api/` (DevTo's `Classify` method is the model).
- Validate the connection and settings first and fail fast with a clear message telling the user what to fix.
- Send an idempotency key where the service supports one (`$"{context.RunId}:{context.StepId}"`, as Mastodon does), so a retried step doesn't post twice. Many services don't support one (DevTo's Forem API, Google Sheets, Pushover): there, a step retried after a timeout may repeat something that actually succeeded, such as appending a row twice. Prefer an upsert-style action where the API allows one (Google Sheets' *Append or Update Row*), and say so in the README.
- Treat `TaskCanceledException` when the caller didn't cancel as a timeout, and `HttpRequestException` as the service being unreachable.
- Log with structured placeholders, never secrets.

### Triggers
- The simplest trigger is a **notification trigger**: derive from `NotificationTriggerBase<TSettings, TOutput, TNotification>` (note the order: settings, output, then the Umbraco notification) and put `[Trigger("<area>.<name>", "Display Name", Group = ..., Icon = ...)]` on it. See `Packages/_Examples/KitchenSink/.../Triggers/`.
- Override `MapEvent(notification)` to return one `TriggerEvent<TOutput>` per affected item, with `TriggerAlias = Alias`, the output, and `IdempotencyKey = GenerateIdempotencyKey(item.Key, 0, item.UpdateDate)` so a duplicate notification doesn't run the automation twice.
- Override `CanHandle(output, settings)` (it's `protected`, and `settings` can be null) to apply each automation's own filters, such as a key prefix.
- Automate discovers the trigger from its attribute and subscribes to the notification for you; there's nothing to register. Output properties reach steps in camelCase as `${ trigger.<property> }`.
- Other kinds exist (`ScheduledTriggerBase` for CRON, `WebhookTriggerBase`, or raising events yourself with `ITriggerDispatcher`), but Automate supplies their output itself; reach for them only when a notification trigger can't do the job.

### Connection validation
- `ValidateAsync` should check required fields, then make one cheap authenticated call and return `Success("Connected as @name")`-style feedback.
- If each check costs the user API quota, make the live check opt-in (see Skoda's `ValidateConnection`).

## Testing

- **xUnit only**: use xUnit's `Assert`, and don't add assertion or mocking libraries (no Shouldly, FluentAssertions, Moq or NSubstitute). One framework keeps tests readable for every contributor and the dependency list short. Some older packages still use Shouldly or Moq; don't copy that into new code.
- Umbraco's own `Umbraco.Automate.Testing` package is fine (it's the official harness, not a mocking library): `ActionTestHarness.For<TAction>().WithService(...).WithSettings(...).WithConnection(alias, settings).ExecuteAsync()` runs an action end to end and returns its `ActionResult`.
- Instead of a mocking library, write small hand-written fakes: a stub `HttpMessageHandler` that returns canned responses and records requests, behind a tiny `IHttpClientFactory` implementation (see `Fakes/StubHttp.cs` in either reference).
- To unit test a connection type's `ValidateAsync`, construct it with `new ConnectionTypeInfrastructure(fakeResolver)`, where the fake implements `IEditableModelResolver` with explicit interface members that throw (validation never uses it). Explicit implementation avoids having to repeat the interface's generic constraint.
- Test each action's success path, each outcome, missing or invalid settings (including an unresolved `$` reference), and how API errors map to categories. Never call the real service.
- Every package has an `<Area>FieldTests` test checking that each setting has a `Label` and `Description` (see the templates), and a package with custom icons has an icon test. Both catch mistakes the backoffice shows silently.
- Group tests in the same folders as the package (`Actions/`, `Connections/`, `Api/`).
- If the connection has a `Client/` front end, add web-test-runner unit tests, and Playwright specs if it needs end-to-end coverage (see Google Sheets).

## README

The package README ships inside the NuGet package and is what nuget.org and the Umbraco Marketplace show, so write it for site developers installing the package. Sections, in order: one-line summary; **Installation** (`dotnet add package`); **Setup** (getting credentials from the service; storing them under `Umbraco:Automate:Secrets:<Area>` / `Umbraco:Automate:Variables:<Area>` with an appsettings example and the environment-variable equivalents, e.g. `Umbraco__Automate__Secrets__<Area>__ApiKey`; creating the connection under **Automation → Settings → Connections** and noting the fields come pre-filled with references); **Actions** (a table of every setting); **Outcomes and outputs**; **Troubleshooting** (the error messages users will actually see, including Automate's *Configuration key '...' not found* for a missing key); **Compatibility**; **Links**. Use absolute URLs for anything outside the package folder, because relative links break on nuget.org.

## Packages without a provider

Triggers and actions with no external service (extra content steps, text and date helpers) are packages in `Packages/` like the rest; there are no top-level folders per kind, because the unit people install and release is a package and most contain several kinds.

- Name the package after its purpose (`Packages/ContentTools/`, `Packages/Text/`) and keep it to one purpose. Never create a catch-all (`Common`, `Utilities`): users would install every step to get one, and every change would release them all.
- Before building a general-purpose step, check it isn't already in Umbraco Automate or in an existing package here; if a step fits an existing general package, add it there.
- Don't create a shared library between packages; duplicate small helpers instead.

Umbraco Automate 17 already includes these, so check the list before building something general-purpose:

| | Built in |
|---|---|
| Content | Triggers: Content Published, Saved and Unpublished (and their batch versions). Actions: Find Content, Get Content, Get Content Property, Update Content Property, Publish Content, Unpublish Content, Notify Editor |
| Media | Triggers: Media Saved, Deleted and Trashed (and batch versions). Action: Update Media Property |
| Members and users | Triggers: Member Saved and Deleted; User Saved, Deleted, Locked, Login Success, Login Failed and Password Changed |
| General | Triggers: Manual, Scheduled, Webhook. Actions: HTTP Request, Send Email, Log Message, Delay, Request Approval, Set Variable, plus the If, Switch, While and For Each steps |

## What a change may touch

Packages are independent: work on one package must never change how another builds, behaves or is released. When creating or changing a package, only change:

- everything under its own `Packages/<Area>/` folder;
- its two lines in `Umbraco.Community.Automate.Demo.slnx`, and its `ProjectReference` in `Umbraco.Community.Automate.Demo/Umbraco.Community.Automate.Demo.csproj`;
- its configuration keys, under its own name, in `Umbraco.Community.Automate.Demo/appsettings.Development.json` (placeholders) and `appsettings.Local.example.json` (the template for real values);
- **new** entries in `Directory.Packages.props` for libraries no package uses yet;
- its row in the root `README.md`, its line in `.github/CODEOWNERS`, its entry in `.github/ISSUE_TEMPLATE/bug-report.yml`, and its `wwwroot/` line in `.gitignore` if it has a `Client/`.

Don't change other packages, the examples, `.github/workflows/`, `.githooks/`, root build or editor settings, this skill, or the version of a library other packages already use. If one of those is genuinely necessary (the package can't build without it), stop and tell the user what and why, and suggest a separate issue or pull request, rather than changing it as part of the package. The exception is when the user explicitly asks for a repo-wide change.

## Adding to an existing connection

- **New action**: add it to `Actions/` with its settings and output (Automate discovers it from its `[Action]` attribute), add tests, and document it in the package README. One action per PR is the norm.
- **New trigger**: same, in `Triggers/`.
- Keep existing aliases, namespaces and public types stable. They're used by people's saved automations and code.

## Migrating an existing package

To bring in a package that lives in its own repo (as WeatherApi did from `SA.Automate.WeatherApi`), or to bring a package already here up to the current conventions, follow [references/migrating.md](references/migrating.md). The essentials:

- Work from the source's latest default branch, and inventory it against these conventions before changing anything.
- Ask first whether it adopts the community naming (recommended) or keeps the author's own package ID, namespaces and aliases. Either way it follows every other convention.
- If the package has been released, ask before renaming aliases, moving configuration or changing the package ID, because saved automations depend on them, and document every such change in a **Migrating from <old package>** README section (see the Mastodon and WeatherApi READMEs).
- Reshape it to the layout and conventions here, keep its behaviour, keep its tests passing, and drop what the repo handles centrally (release workflows, `umbraco-marketplace.json`, `NuGet.config`, licence and icon copies).

## Working notes

This skill is a living document. Add new conventions and lessons learned here (or in the sections above) as the team agrees on them.

- Front-end packages pin Playwright to the version Google Sheets uses (`"overrides": { "playwright": "1.61.0", "playwright-core": "1.61.0" }` in `Client/package.json`), so every `Client/` shares one downloaded browser.
- In a package with a `Client/`, `Client/public/` is copied unchanged into `wwwroot/` by Vite, so `umbraco-package.json` and `icons/` live there and the manifest adds one `bundle` extension for whatever Vite builds (`Client/src/manifests.ts`). Hand-written editors without a build step go straight in `umbraco-package.json` (DevTo).
- Automate discovers `[ConnectionType]`, `[Action]` and `[Trigger]` classes on its own (checked in the Demo site: with the Kitchen Sink's registrations removed, its trigger, actions and connection type all still appear). So composers register services only; registering the types as well is redundant.
- .NET accepts any well-formed culture name (e.g. `xx-YY`), so `CultureInfo.GetCultureInfo` only throws for malformed input. Don't rely on it to reject unknown cultures.
