// Chat API hooks — wraps /api/v1/chat/* endpoints (P2 collaboration module)

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/core/api';
import type { PaginatedResult, PaginationParams } from '@/shared/types/api';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ConversationDto {
  id: string;
  title?: string;
  participantNames: string[];
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
}

export interface ChatMessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: string;
}

export interface CreateConversationRequest {
  participantIds: string[];
  title?: string;
}

export interface SendMessageRequest {
  content: string;
}

// ─── Query keys ───────────────────────────────────────────────────────────────

export const chatQueryKeys = {
  all: ['chat'] as const,
  conversations: ['chat', 'conversations'] as const,
  messages: (id: string, params: PaginationParams) => ['chat', 'messages', id, params] as const,
};

// ─── Queries ──────────────────────────────────────────────────────────────────

/** GET /api/v1/conversations */
export function useConversations() {
  return useQuery({
    queryKey: chatQueryKeys.conversations,
    queryFn: () =>
      apiClient.get<ConversationDto[]>('/conversations').then((r) => r.data),
  });
}

/** GET /api/v1/conversations/{id}/messages */
export function useChatMessages(conversationId: string, params: PaginationParams = {}) {
  return useQuery({
    queryKey: chatQueryKeys.messages(conversationId, params),
    queryFn: () =>
      apiClient
        .get<PaginatedResult<ChatMessageDto>>(`/conversations/${conversationId}/messages`, { params })
        .then((r) => r.data),
    enabled: Boolean(conversationId),
  });
}

// ─── Mutations ────────────────────────────────────────────────────────────────

/** POST /api/v1/conversations */
export function useCreateConversation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: CreateConversationRequest) =>
      apiClient.post<ConversationDto>('/conversations', body).then((r) => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: chatQueryKeys.conversations }),
  });
}

/** POST /api/v1/conversations/{id}/messages */
export function useSendMessage(conversationId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (body: SendMessageRequest) =>
      apiClient
        .post<ChatMessageDto>(`/conversations/${conversationId}/messages`, body)
        .then((r) => r.data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: chatQueryKeys.messages(conversationId, {}) });
      qc.invalidateQueries({ queryKey: chatQueryKeys.conversations });
    },
  });
}
