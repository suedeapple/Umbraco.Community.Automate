using System.Net;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Settings;
using Umbraco.Community.Automate.Example.Api;
using Umbraco.Community.Automate.Example.Connections;
using Umbraco.Community.Automate.Example.Tests.Fakes;
using Xunit;

namespace Umbraco.Community.Automate.Example.Tests.Connections;

public class ExampleConnectionTypeTests
{
    [Fact]
    public async Task Valid_key_connects()
    {
        var handler = new StubHttpMessageHandler(StubHttpMessageHandler.Json(HttpStatusCode.OK, """{ "authenticated": true }"""));

        var result = await CreateSut(handler).ValidateAsync(new ExampleConnectionSettings { ApiKey = "token" }, CancellationToken.None);

        Assert.Equal(ConnectionValidationStatus.Success, result.Status);
        var request = handler.Requests.Single();
        Assert.EndsWith("/bearer", request.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer token", request.Headers.Authorization!.ToString());
    }

    [Fact]
    public async Task Invalid_settings_fail_without_calling_the_service()
    {
        var handler = new StubHttpMessageHandler();

        var result = await CreateSut(handler).ValidateAsync(new ExampleConnectionSettings { ApiKey = "" }, CancellationToken.None);

        Assert.Equal(ConnectionValidationStatus.Failure, result.Status);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Rejected_key_fails_with_the_service_message()
    {
        var handler = new StubHttpMessageHandler(StubHttpMessageHandler.Json(HttpStatusCode.Unauthorized, "denied"));

        var result = await CreateSut(handler).ValidateAsync(new ExampleConnectionSettings { ApiKey = "token" }, CancellationToken.None);

        Assert.Equal(ConnectionValidationStatus.Failure, result.Status);
        Assert.Contains("401", result.Message);
    }

    private static ExampleConnectionType CreateSut(HttpMessageHandler handler)
        => new(new ConnectionTypeInfrastructure(new UnusedModelResolver()), new ExampleClient(new StubHttpClientFactory(handler)));

    /// <summary>ValidateAsync never resolves models; this only satisfies the constructor.</summary>
    private sealed class UnusedModelResolver : IEditableModelResolver
    {
        object IEditableModelResolver.ResolveModel(string alias, Type modelType, object? source, EditableModelSchema? schema) => throw new NotSupportedException();
        TModel IEditableModelResolver.ResolveModel<TModel>(string alias, object? source, EditableModelSchema? schema) => throw new NotSupportedException();
    }
}
