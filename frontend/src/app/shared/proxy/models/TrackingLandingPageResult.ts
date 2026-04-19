/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IpAddressHashPolicy } from './IpAddressHashPolicy';
export type TrackingLandingPageResult = {
  trackingPageId?: string;
  slug?: string | null;
  title?: string | null;
  description?: string | null;
  templateId?: string | null;
  templateName?: string | null;
  templateHtmlContent?: string | null;
  customHtmlContent?: string | null;
  validFromUtc?: string | null;
  validUntilUtc?: string | null;
  captureIpAddress?: boolean;
  ipAddressHashPolicy?: IpAddressHashPolicy;
};
