/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AnalyticsTimeGrain } from './AnalyticsTimeGrain';
import type { CampaignStatus } from './CampaignStatus';
import type { TemplateType } from './TemplateType';
export type AnalyticsFilterDimensions = {
    from: string;
    to: string;
    timeGrain: AnalyticsTimeGrain;
    timeZone: string;
    campaignIds: Array<string>;
    templateTypes: Array<TemplateType>;
    campaignStatuses: Array<CampaignStatus>;
};

