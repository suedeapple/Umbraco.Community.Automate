# Connection templates

Starting points for every file in a new connection, based on the working Mastodon and WeatherApi packages. Replace `Example` / `example` with the connection's area name (PascalCase / camelCase / lowercase as shown) and adjust to the service. Compare with `Packages/WeatherApi/` (or `Packages/Mastodon/` for custom icons) if anything here is unclear; the real code wins if they disagree.

## Contents

- [Package project](#package-project)
- [Directory.Build.props](#directorybuildprops)
- [Test project](#test-project)
- [Configuration](#configuration)
- [Connection settings, validator and type](#connection-settings-validator-and-type)
- [Api client and models](#api-client-and-models)
- [Action, settings and output](#action-settings-and-output)
- [Composer](#composer)
- [Icons and umbraco-package.json](#icons-and-umbraco-packagejson)
- [Tests](#tests)
- [Wiring into the repo](#wiring-into-the-repo)
- [README skeleton](#readme-skeleton)

## Package project

`Packages/Example/Umbraco.Community.Automate.Example/Umbraco.Community.Automate.Example.csproj`. This version serves custom icons from `wwwroot/`. Without a `wwwroot/`, use `Microsoft.NET.Sdk` and drop `AddRazorSupportForMvc` and `StaticWebAssetBasePath`.

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <Title>Umbraco Community Automate Example</Title>
    <Description>Example connection type and actions for Umbraco Automate. One or two sentences on what it lets automations do.</Description>
    <PackageTags>umbraco automate automation example umbraco-marketplace</PackageTags>
    <!-- Serves wwwroot/ as static web assets under this path (umbraco-package.json, icons). -->
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <StaticWebAssetBasePath>App_Plugins/UmbracoCommunityAutomateExample</StaticWebAssetBasePath>
    <PackageIcon>community-automate-128.png</PackageIcon>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Umbraco.Automate.Core" />
    <PackageReference Include="Umbraco.Cms.Core" />
    <PackageReference Include="Umbraco.Cms.Infrastructure" />
    <!-- Versions the package from its git tag (see Directory.Build.props). Without it every
         release packs as 1.0.0 and NuGet skips it as a duplicate. -->
    <PackageReference Include="MinVer" PrivateAssets="All" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="Umbraco.Community.Automate.Example.Tests" />
  </ItemGroup>

  <ItemGroup>
    <!-- README.md is the NuGet / Umbraco Marketplace description. -->
    <None Include="README.md" Pack="true" PackagePath="" />
    <None Include="community-automate-128.png" Pack="true" PackagePath="" />
  </ItemGroup>

</Project>
```

Copy `community-automate-128.png` from another connection. No `Version` attributes: versions live in the root `Directory.Packages.props`.

## Directory.Build.props

`Packages/Example/Umbraco.Community.Automate.Example/Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <Authors>Your Name, Umbraco Community</Authors>
    <PackageProjectUrl>https://github.com/umbraco-community/Umbraco.Community.Automate/tree/main/Packages/Example/Umbraco.Community.Automate.Example</PackageProjectUrl>
    <RepositoryUrl>https://github.com/umbraco-community/Umbraco.Community.Automate</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>

  <PropertyGroup>
    <!-- Releases come from example-v* tags only; height-based pre-release
         versions would react to other packages' commits in this monorepo. -->
    <MinVerTagPrefix>example-v</MinVerTagPrefix>
    <MinVerIgnoreHeight>true</MinVerIgnoreHeight>
  </PropertyGroup>

  <PropertyGroup>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>
</Project>
```

## Test project

`Packages/Example/Umbraco.Community.Automate.Example.Tests/Umbraco.Community.Automate.Example.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <!-- Umbraco's own harness for running actions in tests (ActionTestHarness). -->
    <PackageReference Include="Umbraco.Automate.Testing" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Umbraco.Community.Automate.Example\Umbraco.Community.Automate.Example.csproj" />
  </ItemGroup>

</Project>
```

## Configuration

`Configuration/ExampleConfiguration.cs`. Where the package's values live (nested in Automate's shared sections, which need no registration), the default reference for each credential field, and the service's fixed values.

```csharp
namespace Umbraco.Community.Automate.Example.Configuration;

public static class ExampleConfiguration
{
    public const string VariablesPath = "Umbraco:Automate:Variables:Example";
    public const string SecretsPath = "Umbraco:Automate:Secrets:Example";

    /// <summary>The reference new connections start with, so a key in configuration is used without any typing.</summary>
    public const string ApiKeyReference = "$" + SecretsPath + ":ApiKey";

    // Fixed values: they're the same for every site, so they're constants here, not configuration.

    /// <summary>The service's API.</summary>
    public const string BaseUrl = "https://api.example.com/";
}
```

## Connection settings, validator and type

`Connections/ExampleConnectionSettings.cs`. The credential defaults to its configuration reference, so a new connection opens pre-filled.

```csharp
using Umbraco.Automate.Core.Settings;
using Umbraco.Community.Automate.Example.Configuration;

namespace Umbraco.Community.Automate.Example.Connections;

public sealed class ExampleConnectionSettings
{
    [Field(
        Label = "API key",
        Description = "Generate a key under Settings → API in Example.",
        IsSensitive = true,
        SortOrder = 0)]
    public string ApiKey { get; set; } = ExampleConfiguration.ApiKeyReference;
}
```

`Connections/ExampleConnectionSettingsValidator.cs`. Shared by the connection type and every action, so both report the same message.

```csharp
using Umbraco.Community.Automate.Example.Configuration;

namespace Umbraco.Community.Automate.Example.Connections;

public static class ExampleConnectionSettingsValidator
{
    /// <summary>Returns the first problem found, or null when the settings are valid.</summary>
    public static string? Validate(ExampleConnectionSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(settings?.ApiKey))
            return "An API key is required.";

        if (settings.ApiKey.TrimStart().StartsWith('$'))
            return $"The API key reference '{settings.ApiKey}' could not be resolved. Add the key to configuration at {ExampleConfiguration.SecretsPath}:ApiKey, or enter the key itself on the connection.";

        return null;
    }
}
```

`Connections/ExampleConnectionType.cs`

```csharp
using Umbraco.Automate.Core.Connections;
using Umbraco.Community.Automate.Example.Api;

namespace Umbraco.Community.Automate.Example.Connections;

[ConnectionType("community.example", "Example",
    Description = "Connects Umbraco Automate to Example.",
    Group = "Productivity",
    Icon = "icon-automate-example")]
public sealed class ExampleConnectionType(ConnectionTypeInfrastructure infrastructure, ExampleClient client)
    : ConnectionTypeBase<ExampleConnectionSettings>(infrastructure)
{
    public override async Task<ConnectionValidationResult> ValidateAsync(object? settings, CancellationToken cancellationToken)
    {
        var exampleSettings = settings as ExampleConnectionSettings;

        if (ExampleConnectionSettingsValidator.Validate(exampleSettings) is { } error)
            return ConnectionValidationResult.Failure(error);

        try
        {
            var account = await client.GetCurrentUserAsync(exampleSettings!, cancellationToken);
            return ConnectionValidationResult.Success($"Connected as @{account.Username}.");
        }
        catch (ExampleApiException ex)
        {
            return ConnectionValidationResult.Failure(ex.Message);
        }
    }
}
```

## Api client and models

`Api/ExampleClient.cs`. One place that talks HTTP and turns failures into `StepRunErrorCategory` values, so every action handles errors the same way. Request and response models go in the root `Models/` folder.

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Example.Configuration;
using Umbraco.Community.Automate.Example.Connections;
using Umbraco.Community.Automate.Example.Models;

namespace Umbraco.Community.Automate.Example.Api;

public sealed class ExampleClient(IHttpClientFactory httpClientFactory)
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    public Task<ExampleUser> GetCurrentUserAsync(ExampleConnectionSettings settings, CancellationToken cancellationToken)
        => SendAsync<ExampleUser>(settings, new HttpRequestMessage(HttpMethod.Get, "v1/me"), cancellationToken);

    public async Task<T> SendAsync<T>(ExampleConnectionSettings settings, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(ExampleConfiguration.BaseUrl);
        client.Timeout = RequestTimeout;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ExampleApiException($"Example returned {(int)response.StatusCode}: {body}", Classify(response.StatusCode));
            }

            return (await response.Content.ReadFromJsonAsync<T>(cancellationToken))!;
        }
        catch (HttpRequestException ex)
        {
            throw new ExampleApiException($"Could not reach Example: {ex.Message}", StepRunErrorCategory.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExampleApiException("The request to Example timed out.", StepRunErrorCategory.Timeout, ex);
        }
    }

    // Automate decides whether to retry a step from its category: transient ones
    // (RateLimiting, Timeout, ServiceUnavailable) can retry; the rest fail straight away.
    private static StepRunErrorCategory Classify(HttpStatusCode statusCode) => (int)statusCode switch
    {
        401 or 403 => StepRunErrorCategory.Authentication,
        400 or 404 or 422 => StepRunErrorCategory.Validation,
        408 => StepRunErrorCategory.Timeout,
        429 => StepRunErrorCategory.RateLimiting,
        >= 500 => StepRunErrorCategory.ServiceUnavailable,
        _ => StepRunErrorCategory.InvalidResponse,
    };
}

public sealed class ExampleApiException(string message, StepRunErrorCategory category, Exception? inner = null)
    : Exception(message, inner)
{
    public StepRunErrorCategory Category { get; } = category;
}
```

`Models/ExampleUser.cs`. One file per model, in the root `Models/` folder.

```csharp
namespace Umbraco.Community.Automate.Example.Models;

public sealed class ExampleUser
{
    public string Username { get; set; } = string.Empty;
}
```

## Action, settings and output

`Actions/CreatePostSettings.cs`

```csharp
using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Example.Actions;

public sealed class CreatePostSettings
{
    [Field(Label = "Text",
        Description = "The post text. Supports ${ binding } expressions.",
        SupportsBindings = true,
        SortOrder = 0)]
    public string Text { get; set; } = string.Empty;
}
```

`Actions/CreatePostOutput.cs`. Properties are available to later steps in camelCase: `${ steps.<alias>.url }`.

```csharp
namespace Umbraco.Community.Automate.Example.Actions;

public sealed class CreatePostOutput
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
```

`Actions/CreatePostAction.cs`

```csharp
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Example.Api;
using Umbraco.Community.Automate.Example.Connections;

namespace Umbraco.Community.Automate.Example.Actions;

[Action("community.example.createPost", "Create Example Post",
    ConnectionTypeAlias = "community.example",
    Description = "Creates a post on Example.",
    Group = "Productivity",
    Icon = "icon-automate-example")]
public sealed class CreatePostAction(
    ActionInfrastructure infrastructure,
    ExampleClient client,
    ILogger<CreatePostAction> logger)
    : ActionBase<CreatePostSettings, CreatePostOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var connectionSettings = context.Connection?.GetSettings<ExampleConnectionSettings>();
        if (ExampleConnectionSettingsValidator.Validate(connectionSettings) is { } connectionError)
            return ActionResult.Failed(new InvalidOperationException(connectionError), StepRunErrorCategory.ConfigurationError);

        var settings = context.GetSettings<CreatePostSettings>();
        if (string.IsNullOrWhiteSpace(settings.Text))
            return ActionResult.Failed(new InvalidOperationException("Text is required."), StepRunErrorCategory.Validation);

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/posts")
        {
            Content = JsonContent.Create(new { text = settings.Text }),
        };
        // A retried step must not post twice.
        request.Headers.Add("Idempotency-Key", $"{context.RunId}:{context.StepId}");

        try
        {
            var post = await client.SendAsync<CreatePostOutput>(connectionSettings!, request, cancellationToken);
            logger.LogInformation("Run {RunId}: created Example post {PostId}", context.RunId, post.Id);
            return Success(post);
        }
        catch (ExampleApiException ex)
        {
            return ActionResult.Failed(ex, ex.Category);
        }
    }
}
```

For branching, return an outcome instead: `return SuccessWithOutcome("notFound", new CreatePostOutput());` and document each outcome in the README.

## Composer

`Composers/ExampleComposer.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.Example.Actions;
using Umbraco.Community.Automate.Example.Api;
using Umbraco.Community.Automate.Example.Connections;

namespace Umbraco.Community.Automate.Example.Composers;

public class ExampleComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<ExampleClient>();

        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<ExampleConnectionType>();

        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<CreatePostAction>();

        // No icon registration: Umbraco finds wwwroot/umbraco-package.json on its own.
        // No configuration registration: the default references live under Automate's shared
        // Umbraco:Automate:Secrets / Variables sections, which it resolves out of the box.
    }
}
```

## Icons and umbraco-package.json

Every package with a custom icon registers it the same way, with hand-written files and no C#. Umbraco finds `umbraco-package.json` under `App_Plugins/` on its own, so nothing goes in the composer.

```
wwwroot/                    or Client/public/ in a package with a Client/ (Vite copies it into wwwroot/)
  umbraco-package.json      the package manifest: the icons, plus any other backoffice extensions
  icons/
    icons.js                lists the icons
    example.icon.js         one file per icon: the SVG markup
```

`wwwroot/umbraco-package.json`

```json
{
  "$schema": "https://json.schemastore.org/umbraco-package.json",
  "id": "Umbraco.Community.Automate.Example",
  "name": "Umbraco Community Automate Example",
  "allowTelemetry": true,
  "extensions": [
    {
      "type": "icons",
      "alias": "UmbracoCommunityAutomateExample.Icons",
      "name": "Example Icons",
      "js": "/App_Plugins/UmbracoCommunityAutomateExample/icons/icons.js"
    }
  ]
}
```

The `/App_Plugins/UmbracoCommunityAutomateExample` prefix must match `StaticWebAssetBasePath` in the csproj. Leave out `version`, since nothing would keep it in step with the package version. Other hand-written backoffice extensions (property editor UIs, modals, localization; see DevTo) go in the same `extensions` array. A package with a `Client/` adds a `bundle` extension for what Vite builds (see `Examples/Example/`), but keeps its icons as these plain files rather than in `Client/src/`.

`wwwroot/icons/icons.js`

```js
// Hand-written static web asset, served at /App_Plugins/UmbracoCommunityAutomateExample/icons/.
// Registered by wwwroot/umbraco-package.json. Icon names must match the C# attributes' Icon.
export default [
    {
        name: "icon-automate-example",
        path: () => import("./example.icon.js"),
        keywords: ["example"],
    },
];
```

`wwwroot/icons/example.icon.js` exports the SVG markup as a string: `export default \`<svg ...>...</svg>\`;`. Icon SVGs are inlined into the backoffice page, so prefix any `id` inside the SVG (gradients, clip paths) with the area name. Otherwise another package's icon with the same id can hijack it.

A wrong path or icon name fails silently (the icon is just blank), so add an icon test that links the files to the attributes (see Tests).

## Tests

xUnit only: `Assert` for assertions and hand-written fakes instead of a mocking library.

`Connections/ExampleConnectionSettingsValidatorTests.cs`

```csharp
using Umbraco.Community.Automate.Example.Connections;
using Xunit;

namespace Umbraco.Community.Automate.Example.Tests.Connections;

public class ExampleConnectionSettingsValidatorTests
{
    [Fact]
    public void New_connections_default_to_the_configuration_reference()
        => Assert.Equal("$Umbraco:Automate:Secrets:Example:ApiKey", new ExampleConnectionSettings().ApiKey);

    [Fact]
    public void Missing_api_key_fails()
        => Assert.Equal("An API key is required.", ExampleConnectionSettingsValidator.Validate(new ExampleConnectionSettings { ApiKey = "" }));

    [Fact]
    public void Unresolved_reference_fails_with_a_hint()
        => Assert.Contains("could not be resolved",
            ExampleConnectionSettingsValidator.Validate(new ExampleConnectionSettings { ApiKey = "$Umbraco:Automate:Secrets:Example:ApiKey" }));

    [Fact]
    public void Valid_settings_pass()
        => Assert.Null(ExampleConnectionSettingsValidator.Validate(new ExampleConnectionSettings { ApiKey = "key" }));
}
```

`Fakes/StubHttp.cs`, the fakes for testing anything that calls the API. Hand the factory to `ExampleClient`, then assert on the response and on `handler.Requests`.

```csharp
using System.Net;

namespace Umbraco.Community.Automate.Example.Tests.Fakes;

/// <summary>Returns canned responses in order and records every request sent.</summary>
public sealed class StubHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new(responses);

    public List<HttpRequestMessage> Requests { get; } = [];

    public static HttpResponseMessage Json(HttpStatusCode status, string json)
        => new(status) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_responses.Dequeue());
    }
}

public sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
}
```

```csharp
[Fact]
public async Task Rate_limited_response_is_classified_as_rate_limiting()
{
    var handler = new StubHttpMessageHandler(StubHttpMessageHandler.Json(HttpStatusCode.TooManyRequests, "{}"));
    var client = new ExampleClient(new StubHttpClientFactory(handler));

    var ex = await Assert.ThrowsAsync<ExampleApiException>(
        () => client.GetCurrentUserAsync(new ExampleConnectionSettings { ApiKey = "key" }, CancellationToken.None));

    Assert.Equal(StepRunErrorCategory.RateLimiting, ex.Category);
}
```

`Actions/CreatePostActionTests.cs`. Run an action end to end with Umbraco's `ActionTestHarness`. Give it every service the action's constructor asks for, built on the stub HTTP fakes (here the action takes an `ExampleClient`; one that takes `IHttpClientFactory` directly gets `.WithService<IHttpClientFactory>(...)`):

```csharp
[Fact]
public async Task Missing_text_is_a_validation_error()
{
    var handler = new StubHttpMessageHandler();

    var result = await ActionTestHarness.For<CreatePostAction>()
        .WithService(new ExampleClient(new StubHttpClientFactory(handler)))
        .WithSettings(new CreatePostSettings { Text = "" })
        .WithConnection("community.example", new ExampleConnectionSettings { ApiKey = "key" })
        .ExecuteAsync();

    Assert.Equal(StepRunErrorCategory.Validation, result.ErrorCategory);
    Assert.Empty(handler.Requests);
}
```

To test a connection type's `ValidateAsync`, construct it with a fake model resolver; see `WeatherApiConnectionTypeTests` for a complete example:

```csharp
private sealed class UnusedModelResolver : IEditableModelResolver
{
    object IEditableModelResolver.ResolveModel(string alias, Type modelType, object? source, EditableModelSchema? schema) => throw new NotSupportedException();
    TModel IEditableModelResolver.ResolveModel<TModel>(string alias, object? source, EditableModelSchema? schema) => throw new NotSupportedException();
}

var connectionType = new ExampleConnectionType(new ConnectionTypeInfrastructure(new UnusedModelResolver()), client);
```

`ExampleIconTests.cs`, for a package with custom icons. Nothing else checks that the manifest path, the icon files and the attributes' `Icon` agree, and a mismatch just shows a blank icon. In a package with a `Client/`, read `Client/public` instead of `wwwroot` (see `Examples/Example/`).

```csharp
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Community.Automate.Example.Connections;
using Xunit;

namespace Umbraco.Community.Automate.Example.Tests;

public class ExampleIconTests
{
    private static readonly string Wwwroot = Path.Combine(ProjectDirectory(), "wwwroot");

    [Fact]
    public void Manifest_registers_icons_from_files_that_exist()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Wwwroot, "umbraco-package.json")));
        var icons = Assert.Single(doc.RootElement.GetProperty("extensions").EnumerateArray(), e => e.GetProperty("type").GetString() == "icons");

        // Must match StaticWebAssetBasePath in the csproj, or the backoffice 404s on the module.
        Assert.Equal("/App_Plugins/UmbracoCommunityAutomateExample/icons/icons.js", icons.GetProperty("js").GetString());
        Assert.True(File.Exists(Path.Combine(Wwwroot, "icons", "icons.js")));
        Assert.True(File.Exists(Path.Combine(Wwwroot, "icons", "example.icon.js")));
    }

    [Fact]
    public void Every_connection_type_and_action_uses_a_registered_icon()
    {
        var registered = Regex.Matches(File.ReadAllText(Path.Combine(Wwwroot, "icons", "icons.js")), @"name:\s*""([^""]+)""")
            .Select(m => m.Groups[1].Value)
            .ToHashSet();

        var used = typeof(ExampleConnectionType).Assembly.GetTypes()
            .Select(t => t.GetCustomAttribute<ConnectionTypeAttribute>()?.Icon ?? t.GetCustomAttribute<ActionAttribute>()?.Icon)
            .OfType<string>()
            .Distinct()
            .ToList();

        Assert.NotEmpty(used);
        Assert.All(used, icon => Assert.Contains(icon, registered));
    }

    // The static web assets aren't copied to the test output, so read them from the project.
    private static string ProjectDirectory()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "Umbraco.Community.Automate.Example");
            if (Directory.Exists(Path.Combine(candidate, "wwwroot")))
                return candidate;
        }

        throw new DirectoryNotFoundException("Could not locate the Example project directory.");
    }
}
```

## Wiring into the repo

`Umbraco.Community.Automate.Demo.slnx`, alongside the other connections:

```xml
  <Folder Name="/Packages/Example/">
    <Project Path="Packages/Example/Umbraco.Community.Automate.Example.Tests/Umbraco.Community.Automate.Example.Tests.csproj" />
    <Project Path="Packages/Example/Umbraco.Community.Automate.Example/Umbraco.Community.Automate.Example.csproj" />
  </Folder>
```

`Umbraco.Community.Automate.Demo/Umbraco.Community.Automate.Demo.csproj`:

```xml
    <ProjectReference Include="..\Packages\Example\Umbraco.Community.Automate.Example\Umbraco.Community.Automate.Example.csproj" />
```

Root `README.md` connections table:

```markdown
| [Example](Packages/Example/Umbraco.Community.Automate.Example/README.md) | What it does, in one or two sentences. | Not yet on NuGet: [build from source](#using-a-package-before-its-on-nuget) |
```

## README skeleton

```markdown
# Umbraco.Community.Automate.Example

An Example connection type and actions for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate).

One or two sentences on what automations can do with it, e.g. "Create a post on Example when a blog post is published."

## Installation

    dotnet add package Umbraco.Community.Automate.Example

No further setup required. The composer registers itself automatically.

## Setup

### 1. Get an API key
Where to find it in the service, step by step.

### 2. Store it in configuration
appsettings.json under Umbraco:Automate:Secrets:Example and Umbraco:Automate:Variables:Example, the environment-variable equivalents, and the $-reference to use.

### 3. Create the connection
1. Go to **Automation → Settings → Connections** and create a new **Example** connection.
2. Fill in each field.
3. Click **Test connection**.

## Actions
A table of every action and every setting.

## Outcomes and outputs
Each outcome, and each output as ${ steps.<alias>.<property> } with an example value.

## Troubleshooting
The error messages users will actually see, and what to do about each.

## Compatibility
| Package version | Umbraco Automate | Umbraco CMS |
|---|---|---|

## Links
- [Source code](https://github.com/umbraco-community/Umbraco.Community.Automate/tree/main/Packages/Example/Umbraco.Community.Automate.Example)
- [Report an issue](https://github.com/umbraco-community/Umbraco.Community.Automate/issues)
```
