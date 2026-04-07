// use-chat-signalr — connects to /hubs/chat via SignalR and surfaces real-time messages

import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { chatQueryKeys, type ChatMessageDto } from './chat-api';
import { userManager } from '@/features/auth/auth-provider';

// HubConnection singleton per conversation to avoid duplicate connections.
// ref-counts track how many consumers are active for each conversationId.
// When the last consumer unmounts, the connection is stopped and removed.
const connections: Map<string, signalR.HubConnection> = new Map();
const refCounts: Map<string, number> = new Map();

interface UseChatSignalROptions {
  conversationId: string;
  /** Called when a new message arrives from SignalR */
  onMessage: (msg: ChatMessageDto) => void;
}

// useChatSignalR — joins conversation group, invalidates query cache on new messages
export function useChatSignalR({ conversationId, onMessage }: UseChatSignalROptions) {
  const qc = useQueryClient();
  const onMessageRef = useRef(onMessage);
  onMessageRef.current = onMessage;

  useEffect(() => {
    if (!conversationId) return;

    // Reuse existing connection for same conversation
    let conn = connections.get(conversationId);
    if (!conn) {
      const baseUrl = (import.meta.env.VITE_API_BASE_URL as string) || '';
      conn = new signalR.HubConnectionBuilder()
        .withUrl(`${baseUrl}/hubs/chat`, {
          accessTokenFactory: async () => {
            const user = await userManager.getUser().catch(() => null);
            return user?.access_token ?? '';
          },
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();
      connections.set(conversationId, conn);
    }

    // Increment ref count — tracks how many component instances use this connection
    refCounts.set(conversationId, (refCounts.get(conversationId) ?? 0) + 1);

    // Register message handler
    const handleMessage = (msg: ChatMessageDto) => {
      if (msg.conversationId !== conversationId) return;
      onMessageRef.current(msg);
      // Invalidate to sync any missed messages
      qc.invalidateQueries({ queryKey: chatQueryKeys.messages(conversationId, {}) });
      qc.invalidateQueries({ queryKey: chatQueryKeys.conversations });
    };

    conn.on('ReceiveMessage', handleMessage);

    // Start if not already connected
    if (conn.state === signalR.HubConnectionState.Disconnected) {
      conn
        .start()
        .then(() => conn!.invoke('JoinConversation', conversationId).catch(() => {}))
        .catch(() => {});
    }

    return () => {
      conn!.off('ReceiveMessage', handleMessage);
      // Decrement ref count; stop connection only when last consumer leaves
      const count = (refCounts.get(conversationId) ?? 1) - 1;
      if (count <= 0) {
        conn!.stop().catch(() => {});
        connections.delete(conversationId);
        refCounts.delete(conversationId);
      } else {
        refCounts.set(conversationId, count);
      }
    };
  }, [conversationId, qc]);
}
