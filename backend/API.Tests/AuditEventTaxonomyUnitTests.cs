using Microsoft.AspNetCore.Http;
using QPhising.Api.Infrastructure.Security;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class AuditEventTaxonomyUnitTests
{
    [Theory]
    [InlineData("POST", "/api/templates", 200, "template.save")]
    [InlineData("PUT", "/api/templates/0f8fad5b-d9cb-469f-a165-70867728950e", 200, "template.save")]
    [InlineData("POST", "/api/campaigns/0f8fad5b-d9cb-469f-a165-70867728950e/start", 200, "campaign.start")]
    [InlineData("POST", "/api/campaigns/0f8fad5b-d9cb-469f-a165-70867728950e/pause", 200, "campaign.pause")]
    [InlineData("POST", "/api/campaigns/0f8fad5b-d9cb-469f-a165-70867728950e/complete", 200, "campaign.complete")]
    [InlineData("POST", "/api/campaigns/0f8fad5b-d9cb-469f-a165-70867728950e/cancel", 200, "campaign.cancel")]
    [InlineData("POST", "/api/tracking/pages/0f8fad5b-d9cb-469f-a165-70867728950e/publish", 200, "tracking.publish")]
    [InlineData("POST", "/api/tracking/pages/0f8fad5b-d9cb-469f-a165-70867728950e/archive", 500, "tracking.archive")]
    [InlineData("DELETE", "/api/tracking/pages/0f8fad5b-d9cb-469f-a165-70867728950e", 204, "tracking.delete")]
    public void Classify_ShouldReturnExpectedAction(string method, string path, int statusCode, string expectedAction)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.StatusCode = statusCode;

        var result = AuditEventTaxonomy.Classify(context);

        Assert.NotNull(result);
        Assert.Equal(expectedAction, result!.Action);
    }
}
