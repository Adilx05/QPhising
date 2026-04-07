/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AnalyticsFilterDimensions } from './AnalyticsFilterDimensions';
import type { AnalyticsTrendPoint } from './AnalyticsTrendPoint';
import type { CampaignKpiSummary } from './CampaignKpiSummary';
import type { CampaignStatusBreakdownItem } from './CampaignStatusBreakdownItem';
import type { ClickKpiSummary } from './ClickKpiSummary';
import type { ConversionKpiSummary } from './ConversionKpiSummary';
import type { TaskThroughputKpiSummary } from './TaskThroughputKpiSummary';
import type { TemplateTypeBreakdownItem } from './TemplateTypeBreakdownItem';
export type DashboardKpisResponse = {
    filters: AnalyticsFilterDimensions;
    campaigns: CampaignKpiSummary;
    clicks: ClickKpiSummary;
    conversions: ConversionKpiSummary;
    taskThroughput: TaskThroughputKpiSummary;
    trend: Array<AnalyticsTrendPoint>;
    campaignStatusBreakdown: Array<CampaignStatusBreakdownItem>;
    templateTypeBreakdown: Array<TemplateTypeBreakdownItem>;
};

