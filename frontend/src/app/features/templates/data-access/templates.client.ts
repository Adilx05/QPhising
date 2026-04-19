import {
  TemplateService,
  type CreateTemplateRequest,
  type TemplateResult,
  type UpdateTemplateRequest
} from '../../../shared/proxy';

export interface UpsertTemplateInput {
  name: string;
  htmlContent: string;
  description: string;
  tags: string[];
}

const toUpsertRequest = (
  input: UpsertTemplateInput
): CreateTemplateRequest & UpdateTemplateRequest => ({
  name: input.name,
  htmlContent: input.htmlContent,
  description: input.description,
  tags: input.tags
});

export const listTemplates = async (): Promise<TemplateResult[]> =>
  TemplateService.templateList();

export const createTemplate = async (
  input: UpsertTemplateInput
): Promise<TemplateResult> =>
  TemplateService.templateCreate({
    requestBody: toUpsertRequest(input)
  });

export const updateTemplate = async (
  templateId: string,
  input: UpsertTemplateInput
): Promise<TemplateResult> =>
  TemplateService.templateUpdate({
    templateId,
    requestBody: toUpsertRequest(input)
  });

export const publishTemplate = async (templateId: string): Promise<TemplateResult> =>
  TemplateService.templatePublish({ templateId });

export const archiveTemplate = async (templateId: string): Promise<TemplateResult> =>
  TemplateService.templateArchive({ templateId });

export const deleteTemplate = async (templateId: string): Promise<void> =>
  TemplateService.templateDelete({ templateId });
