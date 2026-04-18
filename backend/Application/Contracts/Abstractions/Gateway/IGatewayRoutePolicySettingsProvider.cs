using QPhising.Application.Contracts.Responses.Gateway;

namespace QPhising.Application.Contracts.Abstractions.Gateway;

public interface IGatewayRoutePolicySettingsProvider
{
    Task<GatewayRoutePolicySettings> GetCurrentAsync(CancellationToken cancellationToken);
}
