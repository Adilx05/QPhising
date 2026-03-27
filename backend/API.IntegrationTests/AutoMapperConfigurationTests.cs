using Xunit;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Features.Campaigns.ActivateCampaign;
using QPhising.Application.Features.Campaigns.ScheduleCampaign;
using QPhising.Application.Features.Campaigns.UpdateCampaign;
using QPhising.Application.Features.Health;
using QPhising.Domain.Campaigns;

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
        var campaign = Campaign.Create(
            "Quarterly Security Awareness",
            TemplateType.Email,
            "<h1>Training</h1>",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7));

        var updateMapped = mapper.Map<UpdateCampaignResponse>(campaign);
        var scheduleMapped = mapper.Map<ScheduleCampaignResponse>(campaign);
        var activateMapped = mapper.Map<ActivateCampaignResponse>(campaign);

        Assert.Equal(campaign.Id, updateMapped.Id);
        Assert.Equal(campaign.Name, updateMapped.Name);
        Assert.Equal(campaign.Status, updateMapped.Status);
        Assert.Equal(campaign.Id, scheduleMapped.Id);
        Assert.Equal(campaign.Status, scheduleMapped.Status);
        Assert.Equal(campaign.Id, activateMapped.Id);
        Assert.Equal(campaign.Status, activateMapped.Status);
    }
}
