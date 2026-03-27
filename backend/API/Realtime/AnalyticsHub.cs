using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace QPhising.API.Realtime;

[Authorize(Policy = AuthorizationPolicies.Viewer)]
public sealed class AnalyticsHub : Hub
{
}

