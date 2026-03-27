using Xunit;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Features.Health;

namespace QPhising.API.IntegrationTests;

public sealed class AutoMapperConfigurationTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public void ApplicationMappings_AreValid()
    {
        using var scope = factory.Services.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        mapper.ConfigurationProvider.AssertConfigurationIsValid();

        var source = new HealthStatus("api", DateTimeOffset.UtcNow, "healthy");
        var mapped = mapper.Map<HealthStatusDto>(source);

        Assert.Equal(source.Service, mapped.Service);
        Assert.Equal(source.Status, mapped.Status);
    }
}
