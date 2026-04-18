import {
  type SaveSetupConfigurationRequest,
  SetupService,
  type SetupStatusResult,
  type TestDatabaseConnectionRequest,
  type TestKeycloakConnectionRequest,
  type TestRedisConnectionRequest,
  type SetupDependencyTestResult
} from '../../../shared/proxy';

export interface SetupConfigurationInput {
  databaseConnectionString: string;
  redisConnectionString: string;
  keycloakAuthority: string;
  keycloakRealm: string;
  keycloakClientId: string;
  keycloakClientSecret: string;
}

export const getSetupStatus = async (): Promise<SetupStatusResult> =>
  SetupService.getStatusSetup();

export const testDatabaseConnection = async (
  connectionString: string
): Promise<SetupDependencyTestResult> => {
  const request: TestDatabaseConnectionRequest = {
    connectionString
  };

  return SetupService.testDatabaseConnectionSetup({ requestBody: request });
};

export const testRedisConnection = async (
  connectionString: string
): Promise<SetupDependencyTestResult> => {
  const request: TestRedisConnectionRequest = {
    connectionString
  };

  return SetupService.testRedisConnectionSetup({ requestBody: request });
};

export const testKeycloakConnection = async (
  authority: string,
  realm: string,
  clientId: string,
  clientSecret: string
): Promise<SetupDependencyTestResult> => {
  const request: TestKeycloakConnectionRequest = {
    authority,
    realm,
    clientId,
    clientSecret
  };

  return SetupService.testKeycloakConnectionSetup({ requestBody: request });
};

export const saveSetupConfiguration = async (
  input: SetupConfigurationInput
): Promise<SetupStatusResult> => {
  const request: SaveSetupConfigurationRequest = {
    databaseConnectionString: input.databaseConnectionString,
    redisConnectionString: input.redisConnectionString,
    keycloakAuthority: input.keycloakAuthority,
    keycloakRealm: input.keycloakRealm,
    keycloakClientId: input.keycloakClientId,
    keycloakClientSecret: input.keycloakClientSecret
  };

  return SetupService.saveSetup({ requestBody: request });
};
