import {
  CampaignService,
  type CampaignResult,
  type CreateCampaignRequest,
  type AddCampaignTargetRequest,
  type ScheduleCampaignRequest
} from '../../../shared/proxy';

export interface CreateCampaignInput {
  name: string;
  templateId: string;
}

export interface AddCampaignTargetInput {
  campaignId: string;
  emailAddress: string;
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
    templateId: input.templateId
  };

  return CampaignService.campaignCreate({ requestBody: request });
};

export const addCampaignTarget = async (
  input: AddCampaignTargetInput
): Promise<CampaignResult> => {
  const request: AddCampaignTargetRequest = {
    emailAddress: input.emailAddress
  };

  return CampaignService.campaignAddTarget({
    campaignId: input.campaignId,
    requestBody: request
  });
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
