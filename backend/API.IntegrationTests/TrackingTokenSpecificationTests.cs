using Microsoft.Extensions.Options;
using QPhising.Application.Common.Abstractions;
using QPhising.Infrastructure.Security;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class TrackingTokenSpecificationTests
{
    [Fact]
    public void IssueToken_Should_Return_ThreeSegment_HmacToken_With_Metadata()
    {
        var service = CreateTokenService();
        Guid campaignId = Guid.NewGuid();

        var issued = service.IssueToken(new TrackingTokenIssueRequest(campaignId, "analyst@company.test", Guid.NewGuid().ToString("N")));

        Assert.Equal(3, issued.Token.Split('.').Length);
        Assert.Equal("HS256", issued.SignatureAlgorithm);
        Assert.Equal(1, issued.Version);
        Assert.True(issued.ExpiresAtUtc > issued.IssuedAtUtc);

        var validation = service.ValidateToken(issued.Token, campaignId);
        Assert.True(validation.IsValid);
        Assert.Equal(campaignId, validation.Claims!.CampaignId);
    }

    [Fact]
    public void ValidateToken_Should_Fail_When_Campaign_Does_Not_Match()
    {
        var service = CreateTokenService();
        Guid campaignId = Guid.NewGuid();
        var issued = service.IssueToken(new TrackingTokenIssueRequest(campaignId, "analyst@company.test", Guid.NewGuid().ToString("N")));

        var validation = service.ValidateToken(issued.Token, Guid.NewGuid());

        Assert.False(validation.IsValid);
        Assert.Equal(TrackingTokenValidationFailure.CampaignMismatch, validation.Failure);
    }

    private static ITrackingTokenService CreateTokenService()
    {
        var options = Options.Create(new TrackingTokenOptions
        {
            SigningKey = "integration-test-signing-key-minimum-32chars",
            ExpirationMinutes = 10,
            Version = 1
        });

        return new HmacTrackingTokenService(options);
    }
}
