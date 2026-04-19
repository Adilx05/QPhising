/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IpAddressHashPolicy } from './IpAddressHashPolicy';
export type TrackingRecentVisitResult = {
  id?: string;
  occurredAtUtc?: string;
  sessionId?: string | null;
  visitorFingerprint?: string | null;
  userAgent?: string | null;
  referrerUrl?: string | null;
  ipHash?: string | null;
  ipAddressHashPolicy?: IpAddressHashPolicy;
};

