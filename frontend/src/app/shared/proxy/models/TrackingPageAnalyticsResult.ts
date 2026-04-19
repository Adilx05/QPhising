/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TrackingAnalyticsSummaryResult } from './TrackingAnalyticsSummaryResult';
import type { TrackingRecentVisitResult } from './TrackingRecentVisitResult';
import type { TrackingVisitTrendPointResult } from './TrackingVisitTrendPointResult';
export type TrackingPageAnalyticsResult = {
  trackingPageId?: string;
  slug?: string | null;
  summary?: TrackingAnalyticsSummaryResult;
  trends?: Array<TrackingVisitTrendPointResult> | null;
  recentVisits?: Array<TrackingRecentVisitResult> | null;
};

