/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GatewayModule } from './GatewayModule';
export type GatewayRoutePolicyDefinitionResult = {
  upstreamPathTemplate?: string | null;
  owner?: GatewayModule;
  requiresAuthentication?: boolean;
  authenticationProviderKey?: string | null;
  forwardAccessToken?: boolean;
  claimsToHeaders?: Record<string, string> | null;
  purpose?: string | null;
};

