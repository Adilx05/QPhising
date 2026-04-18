import {
  ConfigurationService,
  type RuntimeConfigurationResult,
  type SaveRuntimeConfigurationRequest,
  type UpdateRuntimeConfigurationRequest
} from '../../../shared/proxy';

export interface RuntimeConfigurationInput {
  databaseConnectionString: string;
  redisConnectionString: string;
  keycloakAuthority: string;
  keycloakRealm: string;
  keycloakClientId: string;
  keycloakClientSecret: string;
}

export const getRuntimeConfigurationStatus = async (): Promise<RuntimeConfigurationResult> =>
  ConfigurationService.getApiConfiguration();

export const saveRuntimeConfiguration = async (
  input: RuntimeConfigurationInput
): Promise<RuntimeConfigurationResult> => {
  const request: SaveRuntimeConfigurationRequest = {
    databaseConnectionString: input.databaseConnectionString,
    redisConnectionString: input.redisConnectionString,
    keycloakAuthority: input.keycloakAuthority,
    keycloakRealm: input.keycloakRealm,
    keycloakClientId: input.keycloakClientId,
    keycloakClientSecret: input.keycloakClientSecret
  };

  return ConfigurationService.postApiConfiguration({ requestBody: request });
};

export const updateRuntimeConfiguration = async (
  input: Partial<RuntimeConfigurationInput>
): Promise<RuntimeConfigurationResult> => {
  const request: UpdateRuntimeConfigurationRequest = {
    databaseConnectionString: normalizeOptionalValue(input.databaseConnectionString),
    redisConnectionString: normalizeOptionalValue(input.redisConnectionString),
    keycloakAuthority: normalizeOptionalValue(input.keycloakAuthority),
    keycloakRealm: normalizeOptionalValue(input.keycloakRealm),
    keycloakClientId: normalizeOptionalValue(input.keycloakClientId),
    keycloakClientSecret: normalizeOptionalValue(input.keycloakClientSecret)
  };

  return ConfigurationService.patchApiConfiguration({ requestBody: request });
};

const normalizeOptionalValue = (value: string | undefined): string | undefined => {
  if (value === undefined) {
    return undefined;
  }

  const trimmedValue = value.trim();
  return trimmedValue.length > 0 ? trimmedValue : undefined;
};
