/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IpAddressHashPolicy } from './IpAddressHashPolicy';
export type UpdateTrackingPageRequest = {
  slug?: string | null;
  title?: string | null;
  description?: string | null;
  templateId?: string | null;
  customHtmlContent?: string | null;
  validFromUtc?: string | null;
  validUntilUtc?: string | null;
  retentionDays?: number | null;
  captureIpAddress?: boolean | null;
  ipAddressHashPolicy?: IpAddressHashPolicy | null;
  enableBotFiltering?: boolean | null;
  captureUtmParameters?: boolean | null;
};
