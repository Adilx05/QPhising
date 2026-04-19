import { Injectable } from '@angular/core';
import { OidcAuthService } from './oidc-auth.service';

export const identityRoles = ['Admin', 'Operator', 'Viewer'] as const;

export type IdentityRole = (typeof identityRoles)[number];

interface KeycloakRealmAccess {
  roles?: unknown;
}

interface KeycloakResourceAccess {
  roles?: unknown;
}

interface JwtPayloadShape {
  role?: unknown;
  roles?: unknown;
  realm_access?: KeycloakRealmAccess;
  resource_access?: Record<string, KeycloakResourceAccess>;
  given_name?: unknown;
  family_name?: unknown;
  name?: unknown;
  preferred_username?: unknown;
}

const normalizeRole = (value: string): IdentityRole | null => {
  const normalized = value.trim().toLowerCase();

  switch (normalized) {
    case 'admin':
      return 'Admin';
    case 'operator':
      return 'Operator';
    case 'viewer':
      return 'Viewer';
    default:
      return null;
  }
};

const appendRole = (roles: Set<IdentityRole>, value: unknown): void => {
  if (typeof value !== 'string') {
    return;
  }

  const normalized = normalizeRole(value);
  if (normalized !== null) {
    roles.add(normalized);
  }
};

const appendRoleCollection = (roles: Set<IdentityRole>, value: unknown): void => {
  if (!Array.isArray(value)) {
    appendRole(roles, value);
    return;
  }

  for (const role of value) {
    appendRole(roles, role);
  }
};

const resolveRolesFromPayload = (payload: JwtPayloadShape | null): Set<IdentityRole> => {
  const roles = new Set<IdentityRole>();

  if (payload === null) {
    return roles;
  }

  appendRoleCollection(roles, payload.role);
  appendRoleCollection(roles, payload.roles);
  appendRoleCollection(roles, payload.realm_access?.roles);

  if (payload.resource_access && typeof payload.resource_access === 'object') {
    for (const resource of Object.values(payload.resource_access)) {
      appendRoleCollection(roles, resource?.roles);
    }
  }

  return roles;
};

const roleRanks: Record<IdentityRole, number> = {
  Viewer: 1,
  Operator: 2,
  Admin: 3
};

export interface AuthSession {
  accessToken: string | null;
  isAuthenticated: boolean;
  roles: Set<IdentityRole>;
}

export interface AuthUserProfile {
  fullName: string;
  primaryRole: IdentityRole | null;
}

const claimAsTrimmedString = (value: unknown): string | null =>
  typeof value === 'string' && value.trim().length > 0
    ? value.trim()
    : null;

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  public constructor(private readonly oidcAuthService: OidcAuthService) {}

  public getAccessToken(): string | null {
    return this.oidcAuthService.getSession()?.accessToken ?? null;
  }

  public getAuthSession(): AuthSession {
    const oidcSession = this.oidcAuthService.getSession();

    if (oidcSession === null) {
      return {
        accessToken: null,
        isAuthenticated: false,
        roles: new Set<IdentityRole>()
      };
    }

    return {
      accessToken: oidcSession.accessToken,
      isAuthenticated: true,
      roles: resolveRolesFromPayload(this.oidcAuthService.getTokenClaims() as JwtPayloadShape)
    };
  }

  public hasRequiredRole(requiredRole: IdentityRole): boolean {
    const { roles } = this.getAuthSession();

    for (const role of roles) {
      if (roleRanks[role] >= roleRanks[requiredRole]) {
        return true;
      }
    }

    return false;
  }

  public getUserProfile(): AuthUserProfile {
    const session = this.getAuthSession();

    if (!session.isAuthenticated) {
      return {
        fullName: 'Guest User',
        primaryRole: null
      };
    }

    const claims = this.oidcAuthService.getTokenClaims() as JwtPayloadShape;
    const givenName = claimAsTrimmedString(claims.given_name);
    const familyName = claimAsTrimmedString(claims.family_name);
    const name = claimAsTrimmedString(claims.name);
    const preferredUsername = claimAsTrimmedString(claims.preferred_username);

    const fullName = (givenName && familyName)
      ? `${givenName} ${familyName}`
      : name ?? preferredUsername ?? 'Authenticated User';

    const primaryRole = [...session.roles].sort((left, right) => roleRanks[right] - roleRanks[left])[0] ?? null;

    return {
      fullName,
      primaryRole
    };
  }
}
