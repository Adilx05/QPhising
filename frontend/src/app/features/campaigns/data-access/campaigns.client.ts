import {
  CampaignService,
  type CampaignResult,
  type CreateCampaignRequest,
  type ScheduleCampaignRequest
} from '../../../shared/proxy';

export interface CreateCampaignInput {
  name: string;
  trackingPageSlug: string;
  trackingPageTitle: string;
  trackingPageDescription: string | null;
  templateId: string | null;
  htmlContent: string | null;
  validFromUtc: string | null;
  validUntilUtc: string | null;
}

export interface ScheduleCampaignInput {
  campaignId: string;
  startsAtUtc: string;
  endsAtUtc: string | null;
}

export const listCampaigns = async (): Promise<CampaignResult[]> =>
  CampaignService.campaignList();

export const createCampaign = async (
  input: CreateCampaignInput
): Promise<CampaignResult> => {
  const request: CreateCampaignRequest = {
    name: input.name,
    trackingPageSlug: input.trackingPageSlug,
    trackingPageTitle: input.trackingPageTitle,
    trackingPageDescription: input.trackingPageDescription,
    templateId: input.templateId,
    htmlContent: input.htmlContent,
    validFromUtc: input.validFromUtc,
    validUntilUtc: input.validUntilUtc
  };

  return CampaignService.campaignCreate({ requestBody: request });
};

export const scheduleCampaign = async (
  input: ScheduleCampaignInput
): Promise<CampaignResult> => {
  const request: ScheduleCampaignRequest = {
    startsAtUtc: input.startsAtUtc,
    endsAtUtc: input.endsAtUtc
  };

  return CampaignService.campaignSchedule({
    campaignId: input.campaignId,
    requestBody: request
  });
};

export const startCampaign = async (campaignId: string): Promise<CampaignResult> =>
  CampaignService.campaignStart({ campaignId });

export const pauseCampaign = async (campaignId: string): Promise<CampaignResult> =>
  CampaignService.campaignPause({ campaignId });

export const completeCampaign = async (
  campaignId: string
): Promise<CampaignResult> => CampaignService.campaignComplete({ campaignId });

export const cancelCampaign = async (campaignId: string): Promise<CampaignResult> =>
  CampaignService.campaignCancel({ campaignId });

export const deleteCampaign = async (campaignId: string): Promise<void> =>
  CampaignService.campaignDelete({ campaignId });

export const getCampaignById = async (campaignId: string): Promise<CampaignResult> =>
  CampaignService.campaignGetById({ campaignId });
