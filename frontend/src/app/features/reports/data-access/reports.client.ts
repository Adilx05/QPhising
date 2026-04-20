import {
  TrackingAnalyticsService,
  TrackingReportDetailLevel,
  TrackingReportFormat,
  TrackingReportScope,
  type TrackingPageResult
} from '../../../shared/proxy';

export interface ExportReportInput {
  format: TrackingReportFormat;
  scope: TrackingReportScope;
  detailLevel: TrackingReportDetailLevel;
  trackingPageId?: string;
  fromUtc?: string;
  toUtc?: string;
  excludeBots: boolean;
  timezoneOffsetMinutes: number;
}

const resolveExtension = (format: TrackingReportFormat): string =>
  format === TrackingReportFormat._1 ? 'pdf' : 'csv';

const resolveMimeType = (format: TrackingReportFormat): string =>
  format === TrackingReportFormat._1 ? 'application/pdf' : 'text/csv;charset=utf-8';

const toBlob = (payload: unknown, format: TrackingReportFormat): Blob => {
  if (payload instanceof Blob) {
    return payload;
  }

  if (payload instanceof ArrayBuffer) {
    return new Blob([payload], { type: resolveMimeType(format) });
  }

  if (ArrayBuffer.isView(payload)) {
    return new Blob([payload.buffer], { type: resolveMimeType(format) });
  }

  if (typeof payload === 'string') {
    return new Blob([payload], { type: resolveMimeType(format) });
  }

  throw new Error('Export response was not a downloadable file payload.');
};

const triggerDownload = (blob: Blob, filename: string): void => {
  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = objectUrl;
  anchor.download = filename;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(objectUrl);
};

export const exportTrackingReport = async (input: ExportReportInput): Promise<void> => {
  const payload = await TrackingAnalyticsService.trackingAnalyticsExportReport({
    format: input.format,
    scope: input.scope,
    detailLevel: input.detailLevel,
    trackingPageId: input.trackingPageId,
    fromUtc: input.fromUtc,
    toUtc: input.toUtc,
    excludeBots: input.excludeBots,
    timezoneOffsetMinutes: input.timezoneOffsetMinutes
  });

  const blob = toBlob(payload, input.format);
  const now = new Date().toISOString().replace(/[-:TZ.]/g, '').slice(0, 14);
  const filename = `tracking-report-${now}.${resolveExtension(input.format)}`;
  triggerDownload(blob, filename);
};

export const reportScopeRequiresPage = (scope: TrackingReportScope): boolean => scope === TrackingReportScope._1;

export const reportScopeLabel = (scope: TrackingReportScope, selectedPage?: TrackingPageResult): string =>
  scope === TrackingReportScope._1
    ? selectedPage?.slug ?? selectedPage?.title ?? selectedPage?.id ?? 'tracking-page'
    : 'global';
