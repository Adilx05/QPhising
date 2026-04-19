/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IpAddressHashPolicy } from './IpAddressHashPolicy';
export type CaptureVisitRequest = {
  occurredAtUtc?: string;
  sessionId?: string | null;
  visitorFingerprint?: string | null;
  userAgent?: string | null;
  referrerUrl?: string | null;
  ipAddressHashPolicy?: IpAddressHashPolicy;
  deduplicationWindowSeconds?: number;
};
