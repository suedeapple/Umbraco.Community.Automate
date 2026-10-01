# Example connection

A reference connection for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate) that shows every convention in this repo in one small, working package. Copy it when you start a new connection.

It talks to [httpbin.org](https://httpbin.org), a free service that echoes requests back and accepts any API token, so everything works in the Demo site without signing up for anything. It isn't published to NuGet: CI builds and tests it on every pull request so it can't go stale, but it has no release tag prefix and isn't packable.

## What it adds

| | Name | Shows |
|---|---|---|
| Connection | **Example (httpbin.org)** | An API key pre-filled with a configuration reference; **Test connection** making one cheap authenticated call |
| Action | **Send Example Message** | A typed output for later steps, an idempotency key, error categories, and a custom field editor |
| Action | **Check Example Status** | Outcomes (`available`, `notFound`) that later steps branch on, instead of failing |
| Trigger | **Dictionary Item Saved (Example)** | A notification trigger with a per-automation filter |

## Folder by folder

| Folder | What's in it |
|---|---|
| `Actions/` | Each action with its settings and output class. [`SendMessageAction`](Actions/SendMessageAction.cs) is the pattern for "create something" actions; [`CheckStatusAction`](Actions/CheckStatusAction.cs) for "look something up". |
| `Triggers/` | [`DictionaryItemSavedTrigger`](Triggers/DictionaryItemSavedTrigger.cs): `MapEvent` turns an Umbraco notification into trigger events, `CanHandle` filters them per automation. Automate subscribes to the notification for you. |
| `Connections/` | The connection type, its settings (the API key defaults to `$Umbraco:Automate:Secrets:Example:ApiKey`), and a validator shared with the actions. |
| `Composers/` | One composer registering the client, connection type, actions and trigger. |
| `Api/` | [`ExampleClient`](Api/ExampleClient.cs), the only class that talks HTTP: it sends requests and maps every failure to a `StepRunErrorCategory`, which decides whether Automate retries. |
| `Models/` | Request and response models for the service. |
| `Configuration/` | `ExampleConfiguration`: the one place the configuration path, default reference and the service's base URL are defined. |
| `Client/` | The backoffice front end (Vite + Lit): a message editor with a character count, with a unit test. `public/` holds the hand-written `umbraco-package.json` and `icons/`, the same files every package uses for its icons. `npm run build` writes all of it to `wwwroot/`. |
| `wwwroot/` | Build output from `Client/` (not committed), served at `/App_Plugins/UmbracoCommunityAutomateExample/`. |

The tests in `Umbraco.Community.Automate.Example.Tests/` use the same folders, xUnit only, with hand-written HTTP fakes and Umbraco's `ActionTestHarness`.

## Try it in the Demo site

The Demo site references this package and already has a placeholder key at `Umbraco:Automate:Secrets:Example:ApiKey`.

```bash
cd Examples/Example/Umbraco.Community.Automate.Example/Client
npm ci && npm run build
cd ../../../..
dotnet run --project Demo --launch-profile Umbraco.Web.UI
```

1. In **Automation → Settings → Connections**, create an **Example (httpbin.org)** connection. The API key is already filled in; click **Test connection**.
2. Create an automation with the **Dictionary Item Saved (Example)** trigger (optionally set **Key starts with**) and a **Send Example Message** step, with a message such as `Saved ${ trigger.key }`.
3. Publish it, save a dictionary item under **Translation**, and check **Automation → Runs**.

## Run the tests

```bash
dotnet test Examples/Example/Umbraco.Community.Automate.Example.Tests

cd Examples/Example/Umbraco.Community.Automate.Example/Client
npm ci && npm test
```
