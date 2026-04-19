/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TrackingAnalyticsSummaryResult } from './TrackingAnalyticsSummaryResult';
import type { TrackingMetricDefinitionResult } from './TrackingMetricDefinitionResult';
import type { TrackingRecentVisitStreamItemResult } from './TrackingRecentVisitStreamItemResult';
import type { TrackingTopPageResult } from './TrackingTopPageResult';
import type { TrackingVisitTrendPointResult } from './TrackingVisitTrendPointResult';
export type TrackingAnalyticsOverviewResult = {
  summary?: TrackingAnalyticsSummaryResult;
  topPages?: Array<TrackingTopPageResult> | null;
  recentVisits?: Array<TrackingRecentVisitStreamItemResult> | null;
  trends?: Array<TrackingVisitTrendPointResult> | null;
  metricDefinitions?: Array<TrackingMetricDefinitionResult> | null;
};

