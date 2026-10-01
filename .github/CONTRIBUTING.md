# Contributing

Thanks for your interest in contributing! This repo holds community-maintained connections for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate). Each one is a NuGet package that adds a connection type and actions for one external service.

You can contribute in several ways: report a bug, improve a README, add an action to an existing connection, or build a whole new connection. If you're planning something large, [open an issue](https://github.com/umbraco-community/Umbraco.Community.Automate/issues) first so we can agree on the approach before you write the code.

## Contents

- [Prerequisites](#prerequisites)
- [Getting the code](#getting-the-code)
- [Repo layout](#repo-layout)
- [Running the Demo site](#running-the-demo-site)
- [Building and testing](#building-and-testing)
- [Making a change](#making-a-change)
- [Adding a new connection](#adding-a-new-connection)
- [Preventing secret leaks](#preventing-secret-leaks)
- [Releasing a package](#releasing-a-package)

## Prerequisites

| Tool | Needed for |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Everything |
| [gitleaks](https://github.com/gitleaks/gitleaks#installing) | The secret scan that runs before each commit and push (`winget install gitleaks`, `scoop install gitleaks` or `brew install gitleaks`) |
| A trusted HTTPS dev certificate | The Demo site. Run `dotnet dev-certs https --trust` once per machine. |
| [Node.js 22.x](https://nodejs.org/) | Only for connections with a backoffice front end (a `Client/` folder), currently Google Sheets |

Any editor works. Visual Studio, Rider and VS Code (with C# Dev Kit) all open `Umbraco.Community.Automate.slnx`.

## Getting the code

Contributions come in through pull requests from a fork:

```bash
# 1. Fork umbraco-community/Umbraco.Community.Automate on GitHub, then clone your fork
git clone https://github.com/<you>/Umbraco.Community.Automate.git
cd Umbraco.Community.Automate

# 2. Add the original repo as "upstream" so you can pull in other people's changes
git remote add upstream https://github.com/umbraco-community/Umbraco.Community.Automate.git
git fetch upstream

# 3. Turn on the git hooks (once per clone; see "Preventing secret leaks")
./.githooks/setup.sh        # macOS/Linux
.\.githooks\setup.ps1       # Windows
```

Before starting new work, bring your `main` up to date:

```bash
git checkout main
git pull upstream main
git push origin main
```

## Repo layout

```
Connections/                                  one folder per connection
  <Area>/                                     e.g. Mastodon
    Umbraco.Community.Automate.<Area>/        the package (what ships to NuGet)
    Umbraco.Community.Automate.<Area>.Tests/  xUnit tests for the package
Demo/                                         throwaway Umbraco site referencing every package
.github/                                      CI and release workflows, this guide, design notes
.githooks/                                    gitleaks pre-commit and pre-push hooks
Directory.Packages.props                      central NuGet versions for every project
Umbraco.Community.Automate.slnx               the solution
```

Connections live under `Connections/`, the name Umbraco Automate's backoffice uses for them.

### Inside a connection

Every package uses the same folder names, so you always know where to look. The folders for the building blocks Umbraco Automate defines (actions, triggers, connections) and for composers are plural:

| Folder | What goes in it |
|---|---|
| `Actions/` | One class per action, plus its settings and output classes |
| `Triggers/` | One class per trigger, plus its settings and output classes. No connection has triggers yet. |
| `Connections/` | The connection type and its connection settings (and their validator) |
| `Composers/` | The `IComposer` that registers everything, plus any package manifest reader or `IUmbracoBuilder` extensions |
| `Api/` | The C# client for the external service: HTTP client, request and response models, exceptions |
| `Configuration/` | Classes bound from `appsettings.json` (options, config section paths) and their validators |
| `Client/` | The backoffice front end source (Vite + Lit, built by npm into `wwwroot/`). Only for connections that need custom UI. |
| `wwwroot/` | Static backoffice files served under `App_Plugins/` (icons, `umbraco-package.json`) |

Next to those folders sit `Directory.Build.props` (package metadata and the MinVer tag prefix), `README.md` (shipped inside the NuGet package) and usually a package icon. Folders specific to one connection, such as DevTo's `Articles/` and `Content/`, are fine as well. Namespaces follow the folders, and where a test project groups its tests into folders, it uses the same names.

CI finds packages from this layout. Any folder at `<Category>/<Area>/<Project>/` that contains a `Directory.Build.props` counts as a package. It must have a sibling `.Tests` project, and it gets front-end jobs if `Client/package.json` exists. You don't need to edit any workflow file to add a connection.

## Running the Demo site

`Demo/` references every package, so it's where you check a change in the real backoffice. On first run it installs itself into a local SQLite database without any prompts.

### First run

```bash
# Build the Google Sheets backoffice front end. Its output (wwwroot/) is gitignored,
# so without this step the column-list editor won't appear.
cd Connections/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/Client
npm ci
npm run build
cd ../../../..

dotnet run --project Demo --launch-profile Umbraco.Web.UI
```

Then open <https://localhost:44343/umbraco> and log in:

| | |
|---|---|
| Email | `admin@example.com` |
| Password | `password1234` |

The connections are under the **Automation** section in the top navigation. Connections are under **Settings → Connections** in that section's tree, and automations are under **Automations**.

In Visual Studio or Rider, set `Umbraco.Community.Automate.Demo` as the startup project and choose the `Umbraco.Web.UI` launch profile.

### Working on a front end

While you edit a connection's `Client/` code, run its build in watch mode next to the Demo site and refresh the browser after each change:

```bash
cd Connections/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/Client
npm run watch
```

### Using real credentials

The Demo site's `appsettings.Development.json` contains placeholder credentials (`e2e-test`) so that everything boots. To test against the real services, override those values with user secrets. **Never edit the tracked config files with real values.**

```bash
dotnet user-secrets set "Umbraco:Automate:Providers:GoogleSheets:ClientId" "<client-id>" --project Demo
dotnet user-secrets set "Umbraco:Automate:Providers:GoogleSheets:ClientSecret" "<client-secret>" --project Demo
dotnet user-secrets set "Umbraco:Automate:Secrets:Mastodon:AccessToken" "<token>" --project Demo
dotnet user-secrets set "Umbraco:Automate:Secrets:DevTo:ApiKey" "<api-key>" --project Demo
dotnet user-secrets set "Umbraco:Automate:Secrets:Skoda:ApiKey" "<api-key>" --project Demo
dotnet user-secrets set "Umbraco:Automate:Secrets:WeatherApi:ApiKey" "<api-key>" --project Demo
```

New connections are pre-filled with references to exactly these keys, so once they're set you can create a connection without typing any credentials.

For Google Sheets, also add `https://localhost:44343/umbraco/automate/oauth/callback/googlesheets` as an authorised redirect URI on your OAuth client. Skoda's VIN is entered on the connection. Each package's README explains where to get its credentials.

### Resetting the site

Stop the site and delete `Demo/umbraco/Data/`. On the next run it reinstalls from scratch, with no content, connections or automations.

## Building and testing

### .NET

```bash
dotnet build                 # the whole solution
dotnet test                  # every test project

dotnet test Connections/GoogleSheets/Umbraco.Community.Automate.GoogleSheets.Tests   # just one connection
```

CI also runs `dotnet pack` on each package. That catches packaging mistakes that a build won't, such as a missing README or icon. If you touch a `.csproj` or `Directory.Build.props`, run it yourself:

```bash
dotnet pack Connections/GoogleSheets/Umbraco.Community.Automate.GoogleSheets -c Release -o ./pack-check
```

### Front-end unit tests

From a connection's `Client/` folder:

```bash
npm ci
npm test            # web-test-runner; add :watch to re-run on change
```

### End-to-end tests

The Playwright suite lives with Google Sheets (`Connections/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/Client/tests/e2e`). It drives the Demo site in E2E mode, which swaps Google's API for a stub so that no real calls are made.

```bash
cd Connections/GoogleSheets/Umbraco.Community.Automate.GoogleSheets/Client
cp .env.example .env               # defaults match the Demo site
npm ci && npm run build
npx playwright install chromium    # first time only
npm run test:e2e                   # or test:e2e:ui to watch it run
```

Playwright starts the Demo site itself with `AUTOMATE_E2E_MODE=1`. If you already have the site running normally, stop it first. Otherwise Playwright reuses that instance, which isn't in E2E mode, and the tests fail.

### Umbraco version compatibility (DevTo)

The DevTo package ships one build for Umbraco 17 and 18. If you change it, run its compatibility check (it needs bash, so on Windows use Git Bash or WSL):

```bash
./Connections/DevTo/test-umbraco-compat.sh
```

## Making a change

1. **Branch off an up-to-date `main`.** Name branches `<type>/<short-description>`, e.g. `feat/google-sheets-clear-range`, `fix/oauth-token-refresh`, `docs/readme-cleanup`.
2. **Make the change with tests.** New actions and bug fixes should come with unit tests in the package's `.Tests` project. Check UI changes in the Demo site.
3. **Update the docs.** If users would notice the change (a new action, setting, outcome or error message), update that package's `README.md`. It's what NuGet shows.
4. **Commit** using `type(scope): Description`, e.g. `add(action): AppendRowAction — append a row of values to a sheet`, `fix(action): require LookupValue in UpdateRowAction`, `test(action): FindRowActionTests — found/notFound outcomes`. Common types: `add`, `fix`, `refactor`, `test`, `docs`, `chore`.
5. **Push to your fork and open a PR against `umbraco-community/main`.** CI runs backend tests, front-end tests and the Playwright E2E suite, but only for the connections your change touches. Changes outside `Connections/<Area>/` (the Demo site, `Directory.Packages.props`, workflows) run everything. All checks must pass before the PR is merged.

Keep PRs focused. A new action normally lands in its own PR, with its own tests.

If `main` moves on while your PR is open, rebase onto it:

```bash
git fetch upstream
git rebase upstream/main
git push --force-with-lease
```

## Adding a new connection

If you use [Claude Code](https://claude.com/claude-code), the repo includes an `automate-connection` skill ([`.claude/skills/automate-connection/`](../.claude/skills/automate-connection/SKILL.md)). It loads automatically when you work in this repo: ask it to "add a new connection for <service>" and it scaffolds the package in this layout with these conventions. The checklist below is the same process by hand.

1. **Create the projects.** Add `Connections/<Area>/Umbraco.Community.Automate.<Area>/` and `Connections/<Area>/Umbraco.Community.Automate.<Area>.Tests/`. WeatherApi is the best one to copy: an API-key connection, two actions, no front end, and it follows every convention, including tests. Mastodon shows custom icons; Google Sheets shows OAuth and a custom backoffice editor.
2. **Use the standard folders** from [Inside a connection](#inside-a-connection): at least `Actions/`, `Composers/` and `Connections/`, plus `Triggers/`, `Api/`, `Configuration/`, `Client/` and `wwwroot/` when you need them.
3. **Add a `Directory.Build.props`** modelled on an existing package's, with your own `MinVerTagPrefix` (e.g. `newconnection-v`). This file is also what makes CI pick up the package.
4. **Wire it in.** Add both projects to `Umbraco.Community.Automate.slnx`, and reference the package from `Demo/Umbraco.Community.Automate.Demo.csproj`.
5. **Add package versions to `Directory.Packages.props`.** Projects don't specify versions themselves.
6. **Write the package `README.md`**: installation, setup, every setting, outcomes and outputs, troubleshooting and compatibility. Add a row for it to the table in the root `README.md`.
7. **Use placeholder settings only.** If the Demo site needs settings to boot, put obviously fake values in `Demo/appsettings.Development.json`.

Before the first release, follow [Adding a new package to this scheme](#adding-a-new-package-to-this-scheme) below.

## Preventing secret leaks

Never put real credentials (OAuth Client IDs/Secrets, API keys, etc.) into a git-tracked file like `appsettings.Development.json` — even locally, even temporarily. Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) instead, which stores them outside the repo entirely (see [Using real credentials](#using-real-credentials)). User secrets override the tracked placeholder values at runtime when running the Demo site in Development, and nothing about them ever touches git.

### Git hooks

The setup script from [Getting the code](#getting-the-code) needs running once per clone:

```bash
./.githooks/setup.sh        # macOS/Linux
```

```powershell
.\.githooks\setup.ps1       # Windows
```

It points git at the hooks in `.githooks/`, which run [gitleaks](https://github.com/gitleaks/gitleaks) automatically: `pre-commit` scans what you've staged, and `pre-push` scans every commit you're about to push. They're a second, local layer of defence in case a real secret ever ends up staged despite the above. If you set this repo up with the old Lefthook-based script, run the setup script again.

### If a commit or push is blocked

If gitleaks finds something that looks like a secret, your commit or push will fail with output identifying the file and line. To resolve it:

1. Unstage or remove the offending change (`git restore --staged <file>` or edit the file to remove the real value).
2. If it's a real credential you need locally, set it via `dotnet user-secrets set` instead (see above).
3. Re-commit/re-push.

## Releasing a package

Releases are cut by maintainers with push access to this repo. Contributors don't need to do anything here: once your PR is merged, it ships in that package's next release.

This repo uses [MinVer](https://github.com/adamralph/minver) to derive each package's version from git tags — there is no hand-maintained `<Version>` anywhere. Each package has its own tag prefix (e.g. `googlesheets-v`) configured via `MinVerTagPrefix` in that package's own `Directory.Build.props`, so packages in this monorepo version independently: a `googlesheets-v1.0.0` tag only affects the Google Sheets package, even if other packages have had commits in between.

`MinVerIgnoreHeight` is also set to `true` for the same reason: MinVer normally auto-generates a pre-release version between tags based on how many commits have happened since the last tag (its "height"). In a monorepo, that height would react to *other* packages' commits too, producing a misleading version. Since a release here only ever happens by deliberately pushing a tag — never automatically between tags — that auto-increment behaviour isn't needed, so it's turned off.

### How a release happens

1. **Push a tag** matching `<package-slug>-v<semver>` to this repo, e.g.:
   ```bash
   git tag googlesheets-v1.0.0
   git push origin googlesheets-v1.0.0
   ```
   This is the *only* thing that starts a release — merging to `main` never does.
2. `.github/workflows/release.yml` resolves which package the tag belongs to by finding the `Directory.Build.props` whose `MinVerTagPrefix` matches, then runs that package's full CI suite (the same checks as a normal PR, via a call to `ci.yml`) against the tagged commit.
3. If CI passes, it packs the `.nupkg` and `.snupkg` and **pauses for manual approval** — the publish job targets the `nuget-publish` GitHub Environment, which requires a reviewer to approve the run in the GitHub UI before anything is published.
4. Once approved, it publishes to nuget.org using [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) — GitHub's OIDC token is exchanged for a short-lived NuGet API key, so no long-lived API key is ever stored as a repository secret.
5. It creates a GitHub Release for the tag, titled from the package's `<Title>` (e.g. "Umbraco Community Automate Google Sheets v1.0.0"), with GitHub's auto-generated release notes and the `.nupkg`/`.snupkg` attached as downloadable assets.

### Why a fork can't publish a release

Trusted Publishing is bound to this specific repository and workflow — a fork's copy of `release.yml` would request an OIDC token identifying it as `<forker>/Umbraco.Community.Automate`, which nuget.org's trust policy for this package rejects outright, regardless of what the workflow file says. Combined with the fact that a tag pushed to a fork never triggers a workflow run in the upstream repo at all, the actual access boundary for cutting a release is simply **push access to this repository** — the same permission that already lets someone merge to `main`. The `nuget-publish` environment's required-reviewer approval adds a second, deliberate confirmation on top of that.

### Adding a new package to this scheme

1. Add `MinVerTagPrefix` (e.g. `newpackage-v`) and `MinVerIgnoreHeight = true` to that package's own `Directory.Build.props`, following the Google Sheets package as a template.
2. Register a Trusted Publishing policy for the new package on nuget.org, scoped to this repo, the `release.yml` workflow, and the `nuget-publish` environment.
3. No changes to `release.yml` itself are needed — it resolves the package from the tag automatically.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](../LICENSE).
