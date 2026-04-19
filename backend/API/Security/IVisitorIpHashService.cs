using System.Net;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Api.Security;

public interface IVisitorIpHashService
{
    string? ResolveHash(IPAddress? remoteIpAddress, IpAddressHashPolicy hashPolicy);
}
