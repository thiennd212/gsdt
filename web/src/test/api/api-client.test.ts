import { describe, it, expect, vi } from 'vitest';
import { apiClient } from '@/core/api/api-client';

// Mock oidc-client-ts UserManager before importing api-client
vi.mock('oidc-client-ts', () => {
  class MockUserManager {
    getUser = vi.fn().mockResolvedValue(null);
    signinRedirect = vi.fn().mockResolvedValue(undefined);
    signoutRedirect = vi.fn().mockResolvedValue(undefined);
    events = {
      addUserLoaded: vi.fn(),
      addUserUnloaded: vi.fn(),
      addSilentRenewError: vi.fn(),
      removeUserLoaded: vi.fn(),
      removeUserUnloaded: vi.fn(),
      removeSilentRenewError: vi.fn(),
    };
  }
  return {
    UserManager: MockUserManager,
    WebStorageStateStore: class {},
  };
});

// We test the interceptor behaviour by exercising the apiClient directly.
// axios interceptors run in the request/response pipeline — we verify
// the normalized error shape and envelope unwrapping via mock adapter.

describe('apiClient — base config', () => {
  it('is created with baseURL /api/v1 when VITE_API_BASE_URL is unset', () => {
    expect(apiClient.defaults.baseURL).toBe('/api/v1');
  });

  it('has Content-Type: application/json header', () => {
    expect(apiClient.defaults.headers['Content-Type']).toBe('application/json');
  });

  it('has timeout of 30 000ms', () => {
    expect(apiClient.defaults.timeout).toBe(30_000);
  });
});

describe('apiClient — interceptors registration', () => {
  it('has at least one request interceptor attached', () => {
    // Axios stores interceptors internally; verify the manager is non-empty
    // @ts-expect-error accessing internal axios structure for test assertion
    expect(apiClient.interceptors.request.handlers.length).toBeGreaterThan(0);
  });

  it('has at least one response interceptor attached', () => {
    // @ts-expect-error accessing internal axios structure for test assertion
    expect(apiClient.interceptors.response.handlers.length).toBeGreaterThan(0);
  });
});

describe('apiClient — envelope unwrapping (response interceptor)', () => {
  it('unwraps { success, data } envelope and returns inner data', () => {
    // Access the response success interceptor directly
    // @ts-expect-error accessing internal handlers
    const successHandler = apiClient.interceptors.response.handlers[0].fulfilled;
    const fakeResponse = {
      data: { success: true, data: { id: '123', name: 'Test' } },
    };
    const result = successHandler(fakeResponse);
    expect(result.data).toEqual({ id: '123', name: 'Test' });
  });

  it('passes through response when no envelope detected', () => {
    // @ts-expect-error accessing internal handlers
    const successHandler = apiClient.interceptors.response.handlers[0].fulfilled;
    const fakeResponse = { data: [1, 2, 3] };
    const result = successHandler(fakeResponse);
    expect(result.data).toEqual([1, 2, 3]);
  });
});

describe('normalizeError integration via api-client error path', () => {
  it('rejects with normalized AppError shape on HTTP error', async () => {
    // @ts-expect-error accessing internal handlers
    const errorHandler = apiClient.interceptors.response.handlers[0].rejected;

    const axiosError = {
      isAxiosError: true,
      response: {
        status: 403,
        data: { detail_vi: 'Bạn không có quyền thực hiện thao tác này.' },
      },
    };

    await expect(errorHandler(axiosError)).rejects.toMatchObject({
      status: 403,
      message: 'Bạn không có quyền thực hiện thao tác này.',
    });
  });

  it('rejects with 500 normalized error on empty body', async () => {
    // @ts-expect-error accessing internal handlers
    const errorHandler = apiClient.interceptors.response.handlers[0].rejected;

    const axiosError = {
      isAxiosError: true,
      response: { status: 500, data: null },
    };

    await expect(errorHandler(axiosError)).rejects.toMatchObject({
      status: 500,
    });
  });
});
