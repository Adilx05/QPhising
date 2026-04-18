export const identityRoles = ['Admin', 'Operator', 'Viewer'] as const;

export type IdentityRole = (typeof identityRoles)[number];

const runtimeToken = (): string | null => {
  const token = (globalThis as { __QPHISING_ACCESS_TOKEN__?: unknown }).__QPHISING_ACCESS_TOKEN__;

  return typeof token === 'string' && token.trim().length > 0 ? token.trim() : null;
};

const storageToken = (): string | null => {
  const candidates = ['qphising.accessToken', 'qphising_access_token', 'access_token'];

  for (const key of candidates) {
    const localValue = globalThis.localStorage?.getItem(key);
    if (typeof localValue === 'string' && localValue.trim().length > 0) {
      return localValue.trim();
    }

    const sessionValue = globalThis.sessionStorage?.getItem(key);
    if (typeof sessionValue === 'string' && sessionValue.trim().length > 0) {
      return sessionValue.trim();
    }
  }

  return null;
};

const decodeBase64Url = (value: string): string | null => {
  try {
    const padded = value.padEnd(Math.ceil(value.length / 4) * 4, '=');
    const base64 = padded.replace(/-/g, '+').replace(/_/g, '/');

    return globalThis.atob(base64);
  } catch {
    return null;
  }
};

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
}

const decodePayload = (token: string): JwtPayloadShape | null => {
  const parts = token.split('.');

  if (parts.length < 2) {
    return null;
  }

  const payloadJson = decodeBase64Url(parts[1]);
  if (payloadJson === null) {
    return null;
  }

  try {
    return JSON.parse(payloadJson) as JwtPayloadShape;
  } catch {
    return null;
  }
};

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

export const getAccessToken = (): string | null => runtimeToken() ?? storageToken();

export const getAuthSession = (): AuthSession => {
  const accessToken = getAccessToken();

  if (accessToken === null) {
    return {
      accessToken: null,
      isAuthenticated: false,
      roles: new Set<IdentityRole>()
    };
  }

  return {
    accessToken,
    isAuthenticated: true,
    roles: resolveRolesFromPayload(decodePayload(accessToken))
  };
};

export const hasRequiredRole = (requiredRole: IdentityRole): boolean => {
  const { roles } = getAuthSession();

  for (const role of roles) {
    if (roleRanks[role] >= roleRanks[requiredRole]) {
      return true;
    }
  }

  return false;
};
