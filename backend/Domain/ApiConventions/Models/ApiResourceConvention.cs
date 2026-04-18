using QPhising.Domain.ApiConventions.Enums;
using QPhising.Domain.ApiConventions.ValueObjects;

namespace QPhising.Domain.ApiConventions.Models;

/// <summary>
/// Describes naming and versioning constraints for a public API resource.
/// </summary>
public sealed class ApiResourceConvention
{
    public ApiResourceConvention(ApiResourceName resourceName, ApiVersionScope versionScope, string? versionSegment = null)
    {
        ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        VersionScope = versionScope;

        if (versionScope == ApiVersionScope.Versioned)
        {
            if (string.IsNullOrWhiteSpace(versionSegment))
            {
                throw new ArgumentException("Version segment is required for versioned resources.", nameof(versionSegment));
            }

            VersionSegment = versionSegment.Trim();
            return;
        }

        if (!string.IsNullOrWhiteSpace(versionSegment))
        {
            throw new ArgumentException("Unversioned resources cannot define a version segment.", nameof(versionSegment));
        }

        VersionSegment = null;
    }

    public ApiResourceName ResourceName { get; }

    public ApiVersionScope VersionScope { get; }

    public string? VersionSegment { get; }
}
