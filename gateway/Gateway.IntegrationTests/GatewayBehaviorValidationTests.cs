using System.Text.Json;
using Gateway.Configuration;
using Gateway.Correlation;
using Gateway.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;

namespace Gateway.IntegrationTests;

public sealed class GatewayBehaviorValidationTests
{
    private static string ResolveGatewayFilePath(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Unable to resolve gateway file: {fileName}");
    }

    [Fact]
    public void Ocelot_Access_Routes_Should_Require_Bearer_And_Role_Claims()
    {
        var ocelotPath = ResolveGatewayFilePath("ocelot.json");
        using var document = JsonDocument.Parse(File.ReadAllText(ocelotPath));

        var protectedAccessRoutes = document.RootElement
            .GetProperty("Routes")
            .EnumerateArray()
            .Where(route =>
            {
                var upstreamPath = route.GetProperty("UpstreamPathTemplate").GetString() ?? string.Empty;
                return upstreamPath.StartsWith("/api/access", StringComparison.OrdinalIgnoreCase)
                       || upstreamPath.StartsWith("/api/v{version}/access", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        Assert.NotEmpty(protectedAccessRoutes);

        foreach (var route in protectedAccessRoutes)
        {
            Assert.True(route.TryGetProperty("AuthenticationOptions", out var authenticationOptions));
            Assert.Equal("Bearer", authenticationOptions.GetProperty("AuthenticationProviderKey").GetString());

            Assert.True(route.TryGetProperty("RouteClaimsRequirement", out var claimsRequirement));
            var role = claimsRequirement.GetProperty("role").GetString();
            Assert.Contains(role, new[] { "Admin", "Operator", "Viewer" });
        }
    }


    [Fact]
    public void Ocelot_Global_RateLimiting_Should_Be_Disabled_To_Avoid_Duplicate_Throttling_With_Custom_Middleware()
    {
        var ocelotPath = ResolveGatewayFilePath("ocelot.json");
        using var document = JsonDocument.Parse(File.ReadAllText(ocelotPath));

        var globalRateLimitOptions = document.RootElement
            .GetProperty("GlobalConfiguration")
            .GetProperty("RateLimitOptions");

        Assert.True(globalRateLimitOptions.TryGetProperty("EnableRateLimiting", out var enableRateLimiting));
        Assert.False(enableRateLimiting.GetBoolean());
    }

    [Fact]
    public async Task RedisRateLimitingMiddleware_Should_Return_429_With_Standard_Headers_When_Limit_Is_Exceeded()
    {
        var middleware = new RedisRateLimitingMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/campaigns";
        context.Request.Method = HttpMethods.Get;
        context.Request.Headers["X-Client-Id"] = "integration-test-client";

        var options = Options.Create(new RateLimitingOptions
        {
            DefaultWindowSeconds = 60,
            Rules =
            [
                new RateLimitRuleOptions
                {
                    PathPrefix = "/api",
                    Methods = [HttpMethods.Get],
                    Limit = 1,
                    WindowSeconds = 60
                }
            ]
        });

        var redisResults = new RedisResult[] { 2, 30 };

        var databaseMock = new Mock<IDatabase>();
        databaseMock
            .Setup(database => database.ScriptEvaluateAsync(
                It.IsAny<LoadedLuaScript>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisResults);
        databaseMock
            .Setup(database => database.ScriptEvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<RedisKey[]>(),
                It.IsAny<RedisValue[]>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisResults);

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock
            .Setup(multiplexer => multiplexer.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(databaseMock.Object);

        await middleware.InvokeAsync(
            context,
            multiplexerMock.Object,
            options,
            NullLogger<RedisRateLimitingMiddleware>.Instance);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("1", context.Response.Headers["X-RateLimit-Limit"].ToString());
        Assert.Equal("0", context.Response.Headers["X-RateLimit-Remaining"].ToString());
        Assert.Equal("30", context.Response.Headers["X-RateLimit-Reset"].ToString());
        Assert.Equal("30", context.Response.Headers["Retry-After"].ToString());
    }

    [Fact]
    public async Task RedisRateLimitingMiddleware_Should_Not_Apply_Api_Rule_To_ApiHealth_Path()
    {
        var nextCalled = false;
        var middleware = new RedisRateLimitingMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api-health/ready";
        context.Request.Method = HttpMethods.Get;

        var options = Options.Create(new RateLimitingOptions
        {
            DefaultWindowSeconds = 60,
            Rules =
            [
                new RateLimitRuleOptions
                {
                    PathPrefix = "/api",
                    Methods = [HttpMethods.Get],
                    Limit = 1,
                    WindowSeconds = 60
                }
            ]
        });

        var multiplexerMock = new Mock<IConnectionMultiplexer>(MockBehavior.Strict);

        await middleware.InvokeAsync(
            context,
            multiplexerMock.Object,
            options,
            NullLogger<RedisRateLimitingMiddleware>.Instance);

        Assert.True(nextCalled);
        Assert.False(context.Response.Headers.ContainsKey("X-RateLimit-Limit"));
    }

    [Fact]
    public async Task CorrelationIdMiddleware_Should_Propagate_Inbound_Header_To_Response_And_TraceIdentifier()
    {
        var expectedCorrelationId = "test-correlation-id";

        var middleware = new CorrelationIdMiddleware(
            context =>
            {
                Assert.Equal(expectedCorrelationId, context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader].ToString());
                Assert.Equal(expectedCorrelationId, context.TraceIdentifier);
                return Task.CompletedTask;
            },
            NullLogger<CorrelationIdMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader] = expectedCorrelationId;

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedCorrelationId, context.Response.Headers[CorrelationIdMiddleware.CorrelationIdHeader].ToString());
        Assert.Equal(expectedCorrelationId, context.Items[CorrelationIdMiddleware.CorrelationIdItemKey]);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_Should_Not_Throw_When_Response_Has_Already_Started()
    {
        var middleware = new CorrelationIdMiddleware(
            async context =>
            {
                await context.Response.WriteAsync("started");
            },
            NullLogger<CorrelationIdMiddleware>.Instance);

        var context = new DefaultHttpContext();

        var exception = await Record.ExceptionAsync(() => middleware.InvokeAsync(context));

        Assert.Null(exception);
        Assert.True(context.Response.Headers.ContainsKey(CorrelationIdMiddleware.CorrelationIdHeader));
    }

}
