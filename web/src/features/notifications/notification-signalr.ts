// SignalR connection singleton for /hubs/notifications
// Handles connection lifecycle, auto-reconnect, and notification callbacks

import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
  HubConnectionState,
} from '@microsoft/signalr';
import { userManager } from '@/features/auth/auth-provider';

export type NotificationCallback = (notification: {
  id: string;
  title: string;
  body: string;
  deepLink?: string;
}) => void;

let connection: HubConnection | null = null;
const callbacks: Set<NotificationCallback> = new Set();

/** Get or create the SignalR hub connection (singleton) */
function getConnection(): HubConnection {
  if (connection) return connection;

  connection = new HubConnectionBuilder()
    .withUrl('/hubs/notifications', {
      // Auth token passed as query param for WebSocket compatibility
      accessTokenFactory: async () => {
        const user = await userManager.getUser().catch(() => null);
        return user?.access_token ?? '';
      },
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(LogLevel.Warning)
    .build();

  // Forward incoming notifications to all registered callbacks
  connection.on('ReceiveNotification', (notification) => {
    callbacks.forEach((cb) => cb(notification));
  });

  connection.onreconnecting(() => {
    if (import.meta.env.DEV) console.debug('[SignalR] Reconnecting...');
  });

  connection.onreconnected(() => {
    if (import.meta.env.DEV) console.debug('[SignalR] Reconnected');
  });

  connection.onclose(() => {
    if (import.meta.env.DEV) console.debug('[SignalR] Connection closed');
  });

  return connection;
}

/** Start the SignalR connection if not already connected — retries with backoff */
export async function startSignalR(): Promise<void> {
  const conn = getConnection();
  if (conn.state !== HubConnectionState.Disconnected) return;

  const MAX_RETRIES = 3;
  for (let attempt = 0; attempt < MAX_RETRIES; attempt++) {
    try {
      await conn.start();
      if (import.meta.env.DEV) console.debug('[SignalR] Connected to /hubs/notifications');
      return;
    } catch (err) {
      if (attempt === MAX_RETRIES - 1) {
        console.error('[SignalR] Failed to connect after retries:', err);
      } else {
        await new Promise((r) => setTimeout(r, 2000 * (attempt + 1)));
      }
    }
  }
}

/** Stop the SignalR connection */
export async function stopSignalR(): Promise<void> {
  if (connection && connection.state !== HubConnectionState.Disconnected) {
    await connection.stop();
  }
}

/** Register a callback to receive real-time notifications */
export function onNotification(cb: NotificationCallback): () => void {
  callbacks.add(cb);
  return () => callbacks.delete(cb);
}
