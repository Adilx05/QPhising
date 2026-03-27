using System.Net;
using System.Net.Http.Json;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QPhising.Application.Features.Health;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class ValidationPipelineTests
{
    [Fact]
    public async Task Health_Endpoint_Should_Return_ProblemDetails_When_Request_Validation_Fails()
    {
        await using var factory = new ValidationFailureWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.Equal("One or more validation errors occurred.", problem!.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.NotNull(problem.Errors);
        Assert.True(problem.Errors!.Count > 0);
    }

    private sealed class ValidationFailureWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IValidator<GetHealthQuery>, AlwaysFailGetHealthQueryValidator>();
            });
        }
    }

    private sealed class AlwaysFailGetHealthQueryValidator : AbstractValidator<GetHealthQuery>
    {
        public AlwaysFailGetHealthQueryValidator()
        {
            RuleFor(query => query).Must(_ => false).WithMessage("Synthetic validation failure for pipeline verification.");
        }
    }

    private sealed record ValidationProblemResponse(string? Title, int? Status, Dictionary<string, string[]>? Errors);
}
