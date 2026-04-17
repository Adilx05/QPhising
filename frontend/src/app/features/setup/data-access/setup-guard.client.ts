import { SetupAccessState, SetupService, type SetupGuardDecisionResult } from '../../../shared/proxy';

const MAIN_APPLICATION_ACCESS_STATE = SetupAccessState._2;
const DEFAULT_SETUP_PATH = '/setup';

const normalizeRedirectPath = (path: string | null | undefined): string => {
  if (!path || path.trim().length === 0) {
    return DEFAULT_SETUP_PATH;
  }

  return path;
};

export const getSetupGuardDecision = async (): Promise<SetupGuardDecisionResult> =>
  SetupService.getApiSetupGuardDecision();

export const allowsMainApplicationAccess = (decision: SetupGuardDecisionResult): boolean =>
  decision.allowMainApplication === true && decision.accessState === MAIN_APPLICATION_ACCESS_STATE;

export const getRecommendedRedirectPath = (decision: SetupGuardDecisionResult): string =>
  normalizeRedirectPath(decision.recommendedRedirectPath);
