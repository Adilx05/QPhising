namespace QPhising.Api.Contracts.Setup;

public sealed record TestKeycloakConnectionRequest(
    string Authority,
    string Realm,
    string ClientId,
    string ClientSecret);
