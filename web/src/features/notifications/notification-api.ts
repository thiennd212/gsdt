// Notifications API hooks — React Query wrappers for notifications endpoints

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { NotificationDto, UnreadCountDto, PagedResult } from './notification-types';

export const notificationQueryKeys = {
  list: (page: number) => ['notifications', 'list', page] as const,
  unreadCount: ['notifications', 'unread-count'] as const,
};

/** GET /api/v1/notifications (paginated) */
export function useNotifications(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: notificationQueryKeys.list(page),
    queryFn: () =>
      apiClient
        .get<PagedResult<NotificationDto>>('/notifications', { params: { page, pageSize } })
        .then((r) => r.data),
  });
}

/** GET /api/v1/notifications/unread-count */
export function useUnreadCount() {
  return useQuery({
    queryKey: notificationQueryKeys.unreadCount,
    queryFn: () =>
      apiClient.get<UnreadCountDto>('/notifications/unread-count').then((r) => r.data),
    // Refresh every 30s as fallback if SignalR is unavailable
    refetchInterval: 30_000,
  });
}

/** PATCH /api/v1/notifications/{id}/read */
export function useMarkAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      apiClient.patch(`/notifications/${id}/read`).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}

/** POST /api/v1/notifications/read-all */
export function useMarkAllAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => apiClient.post('/notifications/read-all').then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}
