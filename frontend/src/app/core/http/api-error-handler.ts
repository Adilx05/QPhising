import { ApiError, type ProblemDetails } from '../../shared/proxy';

const DEFAULT_ERROR_MESSAGE = 'İşlem tamamlanamadı. Lütfen tekrar deneyin.';
const AUTHENTICATION_REQUIRED_MESSAGE = 'Oturum doğrulanamadı. Lütfen tekrar giriş yapın.';
const AUTHORIZATION_REQUIRED_MESSAGE = 'Bu işlemi gerçekleştirmek için yetkiniz yok.';

interface ProblemDetailsShape {
  title?: unknown;
  detail?: unknown;
  errors?: unknown;
}

const isProblemDetailsShape = (value: unknown): value is ProblemDetailsShape => {
  if (value === null || typeof value !== 'object') {
    return false;
  }

  return true;
};

const resolveValidationDetail = (errors: unknown): string | null => {
  if (errors === null || typeof errors !== 'object') {
    return null;
  }

  const entries = Object.entries(errors as Record<string, unknown>);
  for (const [field, messages] of entries) {
    if (!Array.isArray(messages) || messages.length === 0) {
      continue;
    }

    const firstMessage = messages.find((message) => typeof message === 'string' && message.trim().length > 0);
    if (typeof firstMessage === 'string') {
      return `${field}: ${firstMessage}`;
    }
  }

  return null;
};

const resolveProblemDetailsMessage = (problemDetails: ProblemDetailsShape): string | null => {
  if (typeof problemDetails.detail === 'string' && problemDetails.detail.trim().length > 0) {
    return problemDetails.detail;
  }

  if (typeof problemDetails.title === 'string' && problemDetails.title.trim().length > 0) {
    return problemDetails.title;
  }

  const validationDetail = resolveValidationDetail(problemDetails.errors);
  if (validationDetail !== null) {
    return validationDetail;
  }

  return null;
};

const resolveApiErrorMessage = (error: ApiError): string => {
  if (error.status === 401) {
    return AUTHENTICATION_REQUIRED_MESSAGE;
  }

  if (error.status === 403) {
    return AUTHORIZATION_REQUIRED_MESSAGE;
  }

  if (isProblemDetailsShape(error.body)) {
    const message = resolveProblemDetailsMessage(error.body as ProblemDetails);
    if (message !== null) {
      return message;
    }
  }

  return error.message.trim().length > 0 ? error.message : DEFAULT_ERROR_MESSAGE;
};

export interface ApiErrorResolution {
  message: string;
  isAuthenticationError: boolean;
  isAuthorizationError: boolean;
}

export const resolveApiError = (error: unknown): ApiErrorResolution => {
  if (error instanceof ApiError) {
    return {
      message: resolveApiErrorMessage(error),
      isAuthenticationError: error.status === 401,
      isAuthorizationError: error.status === 403
    };
  }

  if (error instanceof Error && error.message.trim().length > 0) {
    return {
      message: error.message,
      isAuthenticationError: false,
      isAuthorizationError: false
    };
  }

  return {
    message: DEFAULT_ERROR_MESSAGE,
    isAuthenticationError: false,
    isAuthorizationError: false
  };
};
