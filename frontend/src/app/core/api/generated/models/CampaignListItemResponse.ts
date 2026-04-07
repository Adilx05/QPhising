/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CampaignStatus } from './CampaignStatus';
import type { TemplateType } from './TemplateType';
export type CampaignListItemResponse = {
    id: string;
    name: string;
    templateType: TemplateType;
    startDate: string;
    endDate: string;
    status: CampaignStatus;
};

