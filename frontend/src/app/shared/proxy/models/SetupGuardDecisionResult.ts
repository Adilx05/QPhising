/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { SetupAccessState } from './SetupAccessState';
export type SetupGuardDecisionResult = {
  accessState?: SetupAccessState;
  isSetupCompleted?: boolean;
  allowSetupWizard?: boolean;
  allowMainApplication?: boolean;
  recommendedRedirectPath?: string | null;
};

