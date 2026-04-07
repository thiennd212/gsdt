import { QueryClient, QueryCache } from '@tanstack/react-query';
import i18next from 'i18next';
import { getNotificationInstance } from './notification-bridge';
import type { ApiError } from './api-error';

// Singleton QueryClient with GOV-appropriate defaults.
export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error: unknown) => {
      const apiErr = error as ApiError;
      const n = getNotificationInstance();
      if (n) {
        n.error({
          message: apiErr?.message ?? i18next.t('error.loadFailed', { defaultValue: 'Đã xảy ra lỗi khi tải dữ liệu' }),
          description: apiErr?.detail,
          duration: 5,
        });
      }
    },
  }),
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      gcTime: 5 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
    mutations: {
      retry: 0,
      onError: (error: unknown) => {
        if (import.meta.env.DEV) {
          const apiErr = error as ApiError;
          console.error(
            `[API Error] ${apiErr?.status ?? '?'} ${apiErr?.code ?? ''}: ${apiErr?.message ?? error}`,
          );
        }
      },
    },
  },
});
