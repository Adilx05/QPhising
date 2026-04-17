import {
  type AssertProxyContractSyncRequest,
  ProxyValidationService
} from '../../../shared/proxy';

export const assertProxyContractSync = async (
  request: AssertProxyContractSyncRequest
): Promise<void> => ProxyValidationService.proxyValidationAssertSync(request);
