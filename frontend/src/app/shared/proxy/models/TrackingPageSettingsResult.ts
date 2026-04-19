/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IpAddressHashPolicy } from './IpAddressHashPolicy';
export type TrackingPageSettingsResult = {
  retentionDays?: number;
  captureIpAddress?: boolean;
  ipAddressHashPolicy?: IpAddressHashPolicy;
  enableBotFiltering?: boolean;
  captureUtmParameters?: boolean;
};
