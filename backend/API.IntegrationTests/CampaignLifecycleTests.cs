using QPhising.Domain.Campaigns;
using QPhising.Domain.Campaigns.Exceptions;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class CampaignLifecycleTests
{
    [Fact]
    public void Schedule_Should_Move_Draft_To_Scheduled_When_Date_Window_Is_Valid()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Security Awareness Q2",
            TemplateType.Email,
            "<h1>Training</h1>",
            now.AddDays(2),
            now.AddDays(7));

        campaign.Schedule(now);

        Assert.Equal(CampaignStatus.Scheduled, campaign.Status);
    }

    [Fact]
    public void Schedule_Should_Throw_When_StartDate_Is_Not_In_The_Future()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Immediate Campaign",
            TemplateType.Email,
            "<h1>Training</h1>",
            now,
            now.AddDays(3));

        CampaignValidationException exception = Assert.Throws<CampaignValidationException>(() => campaign.Schedule(now));

        Assert.Equal("Campaign can only be scheduled before its start date.", exception.Message);
    }

    [Fact]
    public void Activate_Should_Move_Scheduled_To_Active_Within_Date_Window()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Scheduled Campaign",
            TemplateType.Email,
            "<h1>Training</h1>",
            now.AddDays(1),
            now.AddDays(10));

        campaign.Schedule(now);
        campaign.Activate(now.AddDays(2));

        Assert.Equal(CampaignStatus.Active, campaign.Status);
    }

    [Fact]
    public void Activate_Should_Throw_When_Campaign_Has_Expired()
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = Campaign.Create(
            "Expired Campaign",
            TemplateType.Email,
            "<h1>Training</h1>",
            now.AddDays(1),
            now.AddDays(2));

        campaign.Schedule(now);

        CampaignValidationException exception = Assert.Throws<CampaignValidationException>(() => campaign.Activate(now.AddDays(3)));

        Assert.Equal("Campaign cannot be activated after its end date.", exception.Message);
    }
}
