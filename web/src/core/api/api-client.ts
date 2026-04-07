import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import i18next from 'i18next';
import { userManager } from '@/features/auth/auth-provider';
import { normalizeError } from './api-error';
import { getNotificationInstance } from './notification-bridge';

// Base URL from env — empty string means Vite proxy handles routing to backend
const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

// Module-level flag prevents multiple parallel 401s from triggering multiple signinRedirect() calls.
// No reset needed — page navigates away on redirect; module re-initializes on return.
let isRedirectingToLogin = false;

export const apiClient = axios.create({
  baseURL: BASE_URL || '/api/v1',
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
});

// ─── Request interceptors ────────────────────────────────────────────────────

// 1. Attach auth token + correlation ID to every outgoing request
apiClient.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  // Attempt to get the current OIDC user — may be null when unauthenticated
  const user = await userManager.getUser().catch(() => null);
  if (user?.access_token && !user.expired) {
    config.headers.Authorization = `Bearer ${user.access_token}`;
  }

  // SystemAdmin cross-tenant: attach header if override is a valid GUID
  const tenantOverride = sessionStorage.getItem('admin-tenant-override');
  if (tenantOverride && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(tenantOverride)) {
    config.headers['X-Tenant-Id'] = tenantOverride;
  }

  // Unique ID per request for distributed tracing
  config.headers['X-Correlation-Id'] = crypto.randomUUID();

  return config;
});

// ─── Response interceptors ──────────────────────────────────────────────────

// 2. Unwrap ApiResponse<T> envelope — callers receive `.data` directly
apiClient.interceptors.response.use(
  (response) => {
    // Backend wraps successful payloads: { success, data, message, errors }
    const envelope = response.data as { success?: boolean; data?: unknown };
    if (envelope && typeof envelope === 'object' && 'success' in envelope) {
      response.data = envelope.data;
    }
    return response;
  },
  (error: AxiosError) => {
    const status = error.response?.status ?? 0;
    const body = error.response?.data;

    const normalized = normalizeError(status, body);

    // 401 — session expired; redirect to OIDC login (deduplicated — only one redirect per session)
    if (status === 401 && !isRedirectingToLogin) {
      isRedirectingToLogin = true;
      userManager.signinRedirect().catch(() => {
        window.location.href = '/login';
      });
    }

    // 403 + X-Password-Expired — QĐ742: force password change
    if (status === 403 && error.response?.headers?.['x-password-expired'] === 'true') {
      window.location.href = '/profile?passwordExpired=true';
    } else if (status === 403) {
      getNotificationInstance()?.error({
        key: 'forbidden-403',
        message: i18next.t('error.http.forbidden', { defaultValue: 'Không có quyền truy cập' }),
        description: i18next.t('error.http.forbiddenDetail', { defaultValue: 'Bạn không có quyền thực hiện thao tác này.' }),
        duration: 5,
      });
    }

    // Attach normalized error to the thrown error for upstream consumption
    return Promise.reject(normalized);
  },
);

// Orval mutator — wraps apiClient for generated hooks
// See: https://orval.dev/reference/configuration/output#mutator
export const apiClientMutator = <T>(config: import('axios').AxiosRequestConfig): Promise<T> => {
  return apiClient(config).then(({ data }) => data as T);
};

export default apiClient;
