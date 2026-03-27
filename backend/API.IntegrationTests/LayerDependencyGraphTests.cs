using System.Xml.Linq;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class LayerDependencyGraphTests
{
    [Fact]
    public void Backend_Projects_Should_Follow_Strict_Clean_Architecture_Dependency_Graph()
    {
        var backendRoot = ResolveBackendRoot();

        var dependencies = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Domain"] = GetProjectReferenceNames(Path.Combine(backendRoot, "Domain", "Domain.csproj")),
            ["Application"] = GetProjectReferenceNames(Path.Combine(backendRoot, "Application", "Application.csproj")),
            ["Infrastructure"] = GetProjectReferenceNames(Path.Combine(backendRoot, "Infrastructure", "Infrastructure.csproj")),
            ["API"] = GetProjectReferenceNames(Path.Combine(backendRoot, "API", "API.csproj"))
        };

        Assert.Equal(new HashSet<string>(StringComparer.OrdinalIgnoreCase), dependencies["Domain"]);

        Assert.Equal(
            new HashSet<string>(new[] { "Domain" }, StringComparer.OrdinalIgnoreCase),
            dependencies["Application"]);

        Assert.Equal(
            new HashSet<string>(new[] { "Domain", "Application" }, StringComparer.OrdinalIgnoreCase),
            dependencies["Infrastructure"]);

        Assert.Equal(
            new HashSet<string>(new[] { "Application", "Infrastructure" }, StringComparer.OrdinalIgnoreCase),
            dependencies["API"]);
    }

    private static string ResolveBackendRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "QPhising.Backend.sln");
            if (File.Exists(candidate))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not resolve backend root containing QPhising.Backend.sln.");
    }

    private static HashSet<string> GetProjectReferenceNames(string projectPath)
    {
        var project = XDocument.Load(projectPath);

        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(static includePath => !string.IsNullOrWhiteSpace(includePath))
            .Select(static includePath => Path.GetFileNameWithoutExtension(includePath!))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
