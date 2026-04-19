/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TemplateLifecycleState } from './TemplateLifecycleState';
export type TemplateResult = {
  id?: string;
  name?: string | null;
  htmlContent?: string | null;
  description?: string | null;
  tags?: Array<string> | null;
  lifecycleState?: TemplateLifecycleState;
  version?: number;
  createdAtUtc?: string;
  updatedAtUtc?: string;
};
