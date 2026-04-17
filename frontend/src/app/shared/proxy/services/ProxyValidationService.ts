/* generated using openapi-typescript-codegen -- do not edit */
import type { AssertProxyContractSyncRequest } from '../models/AssertProxyContractSyncRequest';
import type { ProxyContractSyncConflictResponse } from '../models/ProxyContractSyncConflictResponse';
import { OpenAPI } from '../core/OpenAPI';

export class ProxyValidationService {
  public static async proxyValidationAssertSync(
    requestBody?: AssertProxyContractSyncRequest
  ): Promise<void> {
    const response = await fetch(`${OpenAPI.BASE}/api/proxy-validation/assert-sync`, {
      method: 'POST',
      credentials: OpenAPI.WITH_CREDENTIALS ? 'include' : 'same-origin',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(requestBody ?? {})
    });

    if (response.status === 204) {
      return;
    }

    if (response.status === 409) {
      const conflict = (await response.json()) as ProxyContractSyncConflictResponse;
      throw new Error(conflict.message ?? 'Proxy contract drift detected.');
    }

    throw new Error(`Unexpected response status: ${response.status}`);
  }
}
