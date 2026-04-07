import { Injectable } from '@angular/core';

import { AnalyticsService, CampaignsService, ExportsService, TemplatesService } from './generated';

export interface DashboardDataContract {
  kpis: Array<{ title: string; value: string }>;
  trendRows: Array<{ day: string; clicks: number }>;
  campaigns: Array<{
    name: string;
    owner: string;
    status: 'Draft' | 'Scheduled' | 'Active' | 'Ended';
    templateType: 'CredentialHarvest' | 'Attachment' | 'LandingPage';
    startDate: string;
    endDate: string;
    clickRate: number;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class AppApiFacade {
  async getDashboardData(rangeInDays: number): Promise<DashboardDataContract> {
    const to = new Date();
    const from = new Date(to.getTime() - rangeInDays * 24 * 60 * 60 * 1000);
    const response = await AnalyticsService.getApiV1AnalyticsDashboardKpis({
      from: from.toISOString(),
      to: to.toISOString(),
      timeZone: 'UTC',
      timeGrain: 2
    });

    return {
      kpis: [
        { title: 'Total Campaigns', value: String(response.campaigns.total) },
        { title: 'Clicks (Window)', value: String(response.clicks.total) },
        { title: 'Conversion Rate', value: `${Number(response.conversions.conversionRatePercent).toFixed(1)}%` },
        { title: 'Tasks Queued', value: String(response.taskThroughput.enqueued) }
      ],
      trendRows: response.trend.map((point) => ({
        day: new Date(point.bucketStartUtc).toLocaleDateString('en-US', { weekday: 'short' }),
        clicks: Number(point.clicks)
      })),
      campaigns: response.campaignStatusBreakdown.map((entry) => ({
        name: `${entry.status} campaigns`,
        owner: 'System',
        status: this.toUiCampaignStatus(entry.status),
        templateType: 'CredentialHarvest',
        startDate: from.toISOString().slice(0, 10),
        endDate: to.toISOString().slice(0, 10),
        clickRate: 0
      }))
    };
  }

  async listCampaignRows(): Promise<Array<Record<string, string>>> {
    const response = await CampaignsService.getApiV1Campaigns({ skip: 0, take: 50 });

    return response.items.map((item) => ({
      name: item.name,
      template: String(item.templateType),
      status: String(item.status),
      owner: 'N/A'
    }));
  }

  async listTemplateRows(): Promise<Array<Record<string, string>>> {
    const response = await TemplatesService.getApiV1Templates({ pageNumber: 1, pageSize: 50 });

    return response.items.map((item) => ({
      name: item.name,
      type: String(item.type),
      quality: String(item.status),
      owner: 'N/A'
    }));
  }

  async getAnalyticsSummary(rangeInDays: number): Promise<{ kpis: Array<{ title: string; value: string }>; trendRows: Array<Record<string, string>> }> {
    const dashboardData = await this.getDashboardData(rangeInDays);

    return {
      kpis: [
        { title: 'Open Rate', value: `${(dashboardData.trendRows.length > 0 ? 100 : 0).toFixed(1)}%` },
        { title: 'Click Rate', value: dashboardData.kpis[1]?.value ?? '0' },
        { title: 'Credential Submit', value: dashboardData.kpis[2]?.value ?? '0%' }
      ],
      trendRows: dashboardData.trendRows.map((row, index) => ({
        period: `Bucket ${index + 1}`,
        clickRate: row.clicks.toString()
      }))
    };
  }

  async listExportRows(exportJobIds: string[]): Promise<Array<Record<string, string>>> {
    const jobs = await Promise.all(exportJobIds.map((id) => ExportsService.getApiV1Exports({ exportJobId: id })));

    return jobs.map((job) => ({
      report: String(job.exportType),
      format: String(job.format),
      status: String(job.status),
      requestedBy: job.ownerUserId
    }));
  }

  private toUiCampaignStatus(status: number): 'Draft' | 'Scheduled' | 'Active' | 'Ended' {
    switch (status) {
      case 0:
        return 'Draft';
      case 1:
        return 'Scheduled';
      case 2:
        return 'Active';
      default:
        return 'Ended';
    }
  }
}
