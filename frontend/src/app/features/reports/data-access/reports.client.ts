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
  language?: string;
}

interface ReportFilePayload {
  contentType?: string;
  fileName?: string;
  content?: unknown;
}

const resolveExtension = (format: TrackingReportFormat): string =>
  format === TrackingReportFormat._1 ? 'pdf' : 'csv';

const triggerDownload = (blob: Blob, filename: string): void => {
  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = objectUrl;
  anchor.download = filename;
  anchor.rel = 'noopener';
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();

  // Some browsers may resolve PDF object URLs asynchronously after the click event.
  // Revoke on next tick to avoid producing blank pages on downloaded PDFs.
  setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);
};
const resolveMimeType = (format: TrackingReportFormat): string =>
  format === TrackingReportFormat._1 ? 'application/pdf' : 'text/csv;charset=utf-8';

const toBlob = (payload: unknown, format: TrackingReportFormat): Blob => {
  const mimeType = resolveMimeType(format);

  if (payload instanceof Blob) {
    return payload;
  }

  if (payload instanceof ArrayBuffer) {
    return new Blob([payload], { type: mimeType });
  }

  if (ArrayBuffer.isView(payload)) {
    const source = new Uint8Array(
      payload.buffer,
      payload.byteOffset,
      payload.byteLength
    );

    const copy = new Uint8Array(payload.byteLength);
    copy.set(source);

    return new Blob([copy], { type: mimeType });
  }

  if (typeof payload === 'string') {
    return new Blob([payload], { type: mimeType });
  }

  if (payload && typeof payload === 'object') {
    const filePayload = payload as ReportFilePayload;
    if (typeof filePayload.content === 'string') {
      const binary = atob(filePayload.content);
      const bytes = Uint8Array.from(binary, character => character.charCodeAt(0));
      return new Blob([bytes], { type: filePayload.contentType ?? mimeType });
    }
  }

  throw new Error('Export response was not a downloadable file payload.');
};

export const exportTrackingReport = async (input: ExportReportInput): Promise<void> => {
  const payload  = await TrackingAnalyticsService.trackingAnalyticsExportReport({
    format: input.format,
    scope: input.scope,
    detailLevel: input.detailLevel,
    trackingPageId: input.trackingPageId,
    fromUtc: input.fromUtc,
    toUtc: input.toUtc,
    excludeBots: input.excludeBots,
    timezoneOffsetMinutes: input.timezoneOffsetMinutes,
    language: input.language
  });

  const payloadObject = payload && typeof payload === 'object'
    ? payload as ReportFilePayload
    : undefined;
  const now = new Date().toISOString().replace(/[-:TZ.]/g, '').slice(0, 14);
  const filename = payloadObject?.fileName?.trim() || `tracking-report-${now}.${resolveExtension(input.format)}`;
  const blob = toBlob(payload, input.format);
  triggerDownload(blob, filename);
};

export const reportScopeRequiresPage = (scope: TrackingReportScope): boolean => scope === TrackingReportScope._1;

export const reportScopeLabel = (scope: TrackingReportScope, selectedPage?: TrackingPageResult): string =>
  scope === TrackingReportScope._1
    ? selectedPage?.slug ?? selectedPage?.title ?? selectedPage?.id ?? 'tracking-page'
    : 'global';
