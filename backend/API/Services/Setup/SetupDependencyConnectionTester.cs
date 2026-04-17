using System.Data.Common;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.SqlClient;
using QPhising.Application.Contracts.Abstractions.Setup;
using QPhising.Application.Contracts.Responses.Setup;

namespace QPhising.Api.Services.Setup;

public sealed class SetupDependencyConnectionTester : ISetupDependencyConnectionTester
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SetupDependencyConnectionTester(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SetupDependencyTestResult> TestDatabaseAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = Math.Clamp(new SqlConnectionStringBuilder(connectionString).ConnectTimeout, 1, 10)
            };

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await connection.CloseAsync();

            return new SetupDependencyTestResult("database", true);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or ArgumentException or DbException)
        {
            return new SetupDependencyTestResult("database", false, ex.Message);
        }
    }

    public async Task<SetupDependencyTestResult> TestKeycloakAsync(
        string authority,
        string realm,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri))
            {
                return new SetupDependencyTestResult("keycloak", false, "Authority must be a valid absolute URI.");
            }

            var normalizedAuthority = authorityUri.AbsoluteUri.TrimEnd('/');
            var discoveryUri = new Uri($"{normalizedAuthority}/realms/{realm}/.well-known/openid-configuration");

            var httpClient = _httpClientFactory.CreateClient(nameof(SetupDependencyConnectionTester));

            using var discoveryResponse = await httpClient.GetAsync(discoveryUri, cancellationToken);
            if (!discoveryResponse.IsSuccessStatusCode)
            {
                return new SetupDependencyTestResult(
                    "keycloak",
                    false,
                    $"Discovery endpoint returned HTTP {(int)discoveryResponse.StatusCode}.");
            }

            var tokenUri = new Uri($"{normalizedAuthority}/realms/{realm}/protocol/openid-connect/token");
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            });

            using var tokenResponse = await httpClient.PostAsync(tokenUri, content, cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var responseBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                var failureBody = responseBody.Length > 256 ? responseBody[..256] : responseBody;
                return new SetupDependencyTestResult(
                    "keycloak",
                    false,
                    $"Token endpoint returned HTTP {(int)tokenResponse.StatusCode}: {failureBody}");
            }

            return new SetupDependencyTestResult("keycloak", true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new SetupDependencyTestResult("keycloak", false, ex.Message);
        }
    }

    public async Task<SetupDependencyTestResult> TestRedisAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            var (host, port) = ParseRedisEndpoint(connectionString);

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);

            using var stream = tcpClient.GetStream();
            var pingPayload = Encoding.ASCII.GetBytes("*1\r\n$4\r\nPING\r\n");
            await stream.WriteAsync(pingPayload, cancellationToken);

            var responseBuffer = new byte[64];
            var bytesRead = await stream.ReadAsync(responseBuffer, cancellationToken);
            var responseText = Encoding.ASCII.GetString(responseBuffer, 0, bytesRead).Trim();

            var succeeded = responseText.StartsWith("+PONG", StringComparison.OrdinalIgnoreCase) ||
                            responseText.StartsWith("-NOAUTH", StringComparison.OrdinalIgnoreCase);

            return succeeded
                ? new SetupDependencyTestResult("redis", true)
                : new SetupDependencyTestResult("redis", false, $"Unexpected Redis response: {responseText}");
        }
        catch (Exception ex) when (ex is SocketException or InvalidOperationException or ArgumentException)
        {
            return new SetupDependencyTestResult("redis", false, ex.Message);
        }
    }

    private static (string Host, int Port) ParseRedisEndpoint(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Redis connection string is required.", nameof(connectionString));
        }

        var endpointToken = connectionString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(endpointToken))
        {
            throw new InvalidOperationException("Redis connection string does not include an endpoint.");
        }

        var hostAndPort = endpointToken.Split(':', StringSplitOptions.TrimEntries);
        if (hostAndPort.Length == 1)
        {
            return (hostAndPort[0], 6379);
        }

        if (hostAndPort.Length != 2 || !int.TryParse(hostAndPort[1], out var port) || port <= 0 || port > 65535)
        {
            throw new InvalidOperationException("Redis endpoint must use host:port format.");
        }

        return (hostAndPort[0], port);
    }
}
