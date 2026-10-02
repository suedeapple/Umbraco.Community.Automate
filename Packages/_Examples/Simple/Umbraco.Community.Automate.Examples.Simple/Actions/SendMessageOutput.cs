namespace Umbraco.Community.Automate.Examples.Simple.Actions;

/// <summary>
/// What <see cref="SendMessageAction"/> gives later steps, in camelCase:
/// <c>${ steps.&lt;alias&gt;.statusCode }</c>.
/// </summary>
public sealed class SendMessageOutput
{
    /// <summary>The HTTP status code the service answered with.</summary>
    public int StatusCode { get; init; }
}
