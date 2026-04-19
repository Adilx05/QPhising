/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TrackingPagePublishState } from './TrackingPagePublishState';
import type { TrackingPageSettingsResult } from './TrackingPageSettingsResult';
export type TrackingPageResult = {
  id?: string;
  slug?: string | null;
  title?: string | null;
  description?: string | null;
  ownerId?: string | null;
  templateId?: string | null;
  customHtmlContent?: string | null;
  validFromUtc?: string | null;
  validUntilUtc?: string | null;
  publishState?: TrackingPagePublishState;
  settings?: TrackingPageSettingsResult;
  createdAtUtc?: string;
  updatedAtUtc?: string;
};

