import { ApiError, type ProblemDetails } from '../../shared/proxy';
import { resolveApiError } from './api-error-handler';

function createApiError(status: number, body: ProblemDetails | null, message: string): ApiError {
  const request = {
    method: 'GET' as const,
    url: '/api/test'
  };
  const response = {
    url: '/api/test',
    ok: status >= 200 && status < 300,
    status,
    statusText: '',
    body
  };
  return new ApiError(request, response, message);
}

const EMPTY_REQUEST = { method: 'GET' as const, url: '' };

describe('resolveApiError', () => {
  describe('ApiError with HTTP status codes', () => {
    it('should return authentication error for 401', () => {
      const error = createApiError(401, null, 'Unauthorized');
      const result = resolveApiError(error);

      expect(result.message).toBe('Oturum doğrulanamadı. Lütfen tekrar giriş yapın.');
      expect(result.isAuthenticationError).toBeTrue();
      expect(result.isAuthorizationError).toBeFalse();
    });

    it('should return authorization error for 403', () => {
      const error = createApiError(403, null, 'Forbidden');
      const result = resolveApiError(error);

      expect(result.message).toBe('Bu işlemi gerçekleştirmek için yetkiniz yok.');
      expect(result.isAuthenticationError).toBeFalse();
      expect(result.isAuthorizationError).toBeTrue();
    });

    it('should use ProblemDetails title when available', () => {
      const body: ProblemDetails = { title: 'Validation Error', detail: 'The request failed validation.' };
      const error = createApiError(400, body, 'Bad Request');
      const result = resolveApiError(error);

      expect(result.message).toBe('The request failed validation.');
      expect(result.isAuthenticationError).toBeFalse();
      expect(result.isAuthorizationError).toBeFalse();
    });

    it('should fall back to detail when title is missing', () => {
      const body: ProblemDetails = { detail: 'The request failed.' };
      const error = createApiError(400, body, 'Bad Request');
      const result = resolveApiError(error);

      expect(result.message).toBe('The request failed.');
    });

    it('should extract validation error from ProblemDetails errors', () => {
      const body: ProblemDetails = {
        errors: {
          name: ['Name is required', 'Name must be at least 2 characters'],
          email: ['Email is invalid']
        }
      };
      const error = createApiError(422, body, 'Unprocessable Entity');
      const result = resolveApiError(error);

      expect(result.message).toBe('name: Name is required');
    });

    it('should fall back to error message when ProblemDetails is empty', () => {
      const error = createApiError(500, {}, 'Internal Server Error');
      const result = resolveApiError(error);

      expect(result.message).toBe('Internal Server Error');
    });

    it('should use default message when error message is empty and no body', () => {
      const error = createApiError(500, null, '');
      const result = resolveApiError(error);

      expect(result.message).toBe('İşlem tamamlanamadı. Lütfen tekrar deneyin.');
    });
  });

  describe('non-ApiError inputs', () => {
    it('should use message from regular Error', () => {
      const error = new Error('Something went wrong');
      const result = resolveApiError(error);

      expect(result.message).toBe('Something went wrong');
      expect(result.isAuthenticationError).toBeFalse();
      expect(result.isAuthorizationError).toBeFalse();
    });

    it('should use default message for Error with empty message', () => {
      const error = new Error('');
      const result = resolveApiError(error);

      expect(result.message).toBe('İşlem tamamlanamadı. Lütfen tekrar deneyin.');
    });

    it('should use default message for null', () => {
      const result = resolveApiError(null);

      expect(result.message).toBe('İşlem tamamlanamadı. Lütfen tekrar deneyin.');
    });

    it('should use default message for undefined', () => {
      const result = resolveApiError(undefined);

      expect(result.message).toBe('İşlem tamamlanamadı. Lütfen tekrar deneyin.');
    });

    it('should use default message for non-Error objects without message', () => {
      const result = resolveApiError({ someField: 'value' });

      expect(result.message).toBe('İşlem tamamlanamadı. Lütfen tekrar deneyin.');
    });
  });
});
