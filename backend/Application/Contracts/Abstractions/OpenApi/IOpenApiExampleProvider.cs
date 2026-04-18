namespace QPhising.Application.Contracts.Abstractions.OpenApi;

/// <summary>
/// Provides canonical request/response examples for an API contract.
/// </summary>
public interface IOpenApiExampleProvider<out TResponse>
{
    TResponse BuildSuccessExample();
}

/// <summary>
/// Provides canonical request and response examples for an API contract.
/// </summary>
public interface IOpenApiExampleProvider<TRequest, out TResponse> : IOpenApiExampleProvider<TResponse>
{
    TRequest BuildRequestExample();
}
