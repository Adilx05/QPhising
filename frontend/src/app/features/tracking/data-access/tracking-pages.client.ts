import {
  TrackingService,
  type CreateTrackingPageRequest,
  type TrackingPageAnalyticsResult,
  type TrackingPageResult,
  type UpdateTrackingPageRequest,
  IpAddressHashPolicy
} from '../../../shared/proxy';

export interface UpsertTrackingPageInput {
  slug: string;
  title: string;
  description: string;
  customHtmlContent: string;
  validFromUtc: string | null;
  validUntilUtc: string | null;
  ownerId: string;
  templateId: string;
  retentionDays: number;
  captureIpAddress: boolean;
  ipAddressHashPolicy: IpAddressHashPolicy;
  enableBotFiltering: boolean;
  captureUtmParameters: boolean;
}

export interface TrackingAnalyticsFilterInput {
  trackingPageId: string;
  fromUtc: string | null;
  toUtc: string | null;
  trendBucketSizeMinutes: number;
  recentVisitLimit: number;
}

const toCreateRequest = (
  input: UpsertTrackingPageInput
): CreateTrackingPageRequest => ({
  slug: input.slug,
  title: input.title,
  description: input.description,
  customHtmlContent: input.customHtmlContent,
  validFromUtc: input.validFromUtc,
  validUntilUtc: input.validUntilUtc,
  ownerId: input.ownerId,
  templateId: input.templateId || undefined,
  retentionDays: input.retentionDays,
  captureIpAddress: input.captureIpAddress,
  ipAddressHashPolicy: input.ipAddressHashPolicy,
  enableBotFiltering: input.enableBotFiltering,
  captureUtmParameters: input.captureUtmParameters
});

const toUpdateRequest = (
  input: UpsertTrackingPageInput
): UpdateTrackingPageRequest => ({
  slug: input.slug,
  title: input.title,
  description: input.description,
  customHtmlContent: input.customHtmlContent,
  validFromUtc: input.validFromUtc,
  validUntilUtc: input.validUntilUtc,
  templateId: input.templateId || undefined,
  retentionDays: input.retentionDays,
  captureIpAddress: input.captureIpAddress,
  ipAddressHashPolicy: input.ipAddressHashPolicy,
  enableBotFiltering: input.enableBotFiltering,
  captureUtmParameters: input.captureUtmParameters
});

export const listTrackingPages = async (): Promise<TrackingPageResult[]> =>
  TrackingService.trackingPageList();

export const createTrackingPage = async (
  input: UpsertTrackingPageInput
): Promise<TrackingPageResult> =>
  TrackingService.trackingPageCreate({ requestBody: toCreateRequest(input) });

export const updateTrackingPage = async (
  trackingPageId: string,
  input: UpsertTrackingPageInput
): Promise<TrackingPageResult> =>
  TrackingService.trackingPageUpdate({
    trackingPageId,
    requestBody: toUpdateRequest(input)
  });

export const publishTrackingPage = async (
  trackingPageId: string
): Promise<TrackingPageResult> => TrackingService.trackingPagePublish({ trackingPageId });

export const archiveTrackingPage = async (
  trackingPageId: string
): Promise<TrackingPageResult> => TrackingService.trackingPageArchive({ trackingPageId });

export const deleteTrackingPage = async (trackingPageId: string): Promise<void> =>
  TrackingService.trackingPageDelete({ trackingPageId });

export const getTrackingPageAnalytics = async (
  input: TrackingAnalyticsFilterInput
): Promise<TrackingPageAnalyticsResult> =>
  TrackingService.trackingPageGetAnalytics({
    trackingPageId: input.trackingPageId,
    fromUtc: input.fromUtc ?? undefined,
    toUtc: input.toUtc ?? undefined,
    trendBucketSizeMinutes: input.trendBucketSizeMinutes,
    recentVisitLimit: input.recentVisitLimit
  });
