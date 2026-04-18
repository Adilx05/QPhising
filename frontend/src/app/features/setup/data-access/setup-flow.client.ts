import {
  type SaveSetupConfigurationRequest,
  SetupService,
  type SetupStatusResult,
  type TestDatabaseConnectionRequest,
  type TestKeycloakConnectionRequest,
  type TestRedisConnectionRequest,
  type SetupDependencyTestResult
} from '../../../shared/proxy';
import { resolveProxyServiceMethod } from './proxy-service-method.resolver';


const getApiSetupStatus = resolveProxyServiceMethod<[], Promise<SetupStatusResult>>(
  SetupService,
  'getApiSetupStatus'
);
const postApiSetupTestDb = resolveProxyServiceMethod<
  [{ requestBody?: TestDatabaseConnectionRequest }],
  Promise<SetupDependencyTestResult>
>(SetupService, 'postApiSetupTestDb');
const postApiSetupTestRedis = resolveProxyServiceMethod<
  [{ requestBody?: TestRedisConnectionRequest }],
  Promise<SetupDependencyTestResult>
>(SetupService, 'postApiSetupTestRedis');
const postApiSetupTestKeycloak = resolveProxyServiceMethod<
  [{ requestBody?: TestKeycloakConnectionRequest }],
  Promise<SetupDependencyTestResult>
>(SetupService, 'postApiSetupTestKeycloak');
const postApiSetupSave = resolveProxyServiceMethod<
  [{ requestBody?: SaveSetupConfigurationRequest }],
  Promise<SetupStatusResult>
>(SetupService, 'postApiSetupSave');

export interface SetupConfigurationInput {
  databaseConnectionString: string;
  redisConnectionString: string;
  keycloakAuthority: string;
  keycloakRealm: string;
  keycloakClientId: string;
  keycloakClientSecret: string;
}

export const getSetupStatus = async (): Promise<SetupStatusResult> =>
  getApiSetupStatus();

export const testDatabaseConnection = async (
  connectionString: string
): Promise<SetupDependencyTestResult> => {
  const request: TestDatabaseConnectionRequest = {
    connectionString
  };

  return postApiSetupTestDb({ requestBody: request });
};

export const testRedisConnection = async (
  connectionString: string
): Promise<SetupDependencyTestResult> => {
  const request: TestRedisConnectionRequest = {
    connectionString
  };

  return postApiSetupTestRedis({ requestBody: request });
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

  return postApiSetupTestKeycloak({ requestBody: request });
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

  return postApiSetupSave({ requestBody: request });
};
