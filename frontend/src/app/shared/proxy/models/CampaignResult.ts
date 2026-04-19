/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CampaignLifecycleState } from './CampaignLifecycleState';
export type CampaignResult = {
  id?: string;
  name?: string | null;
  templateId?: string;
  lifecycleState?: CampaignLifecycleState;
  startsAtUtc?: string | null;
  endsAtUtc?: string | null;
  createdAtUtc?: string;
  updatedAtUtc?: string;
};

