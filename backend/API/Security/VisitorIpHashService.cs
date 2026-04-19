using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Api.Security;

public sealed class VisitorIpHashService : IVisitorIpHashService
{
    private readonly VisitorIpHashOptions _options;

    public VisitorIpHashService(IOptions<VisitorIpHashOptions> options)
    {
        _options = options.Value;
    }

    public string? ResolveHash(IPAddress? remoteIpAddress, IpAddressHashPolicy hashPolicy)
    {
        if (hashPolicy == IpAddressHashPolicy.None || remoteIpAddress is null)
        {
            return null;
        }

        return hashPolicy switch
        {
            IpAddressHashPolicy.PlainText => remoteIpAddress.ToString(),
            IpAddressHashPolicy.Sha256 => ComputeSha256(remoteIpAddress),
            _ => throw new ArgumentOutOfRangeException(nameof(hashPolicy), hashPolicy, "Unsupported IP hash policy.")
        };
    }

    private string ComputeSha256(IPAddress remoteIpAddress)
    {
        var pepper = _options.HashPepper.Trim();
        var material = string.IsNullOrWhiteSpace(pepper)
            ? remoteIpAddress.ToString()
            : $"{remoteIpAddress}|{pepper}";

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
