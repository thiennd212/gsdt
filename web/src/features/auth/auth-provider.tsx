import { createContext, useCallback, useEffect, useRef, useState } from 'react';
import { UserManager, type User } from 'oidc-client-ts';
import { getNotificationInstance } from '@/core/api/notification-bridge';
import { oidcConfig } from './auth-config';
import { stopSignalR } from '@/features/notifications/notification-signalr';
import type { ReactNode } from 'react';

export interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

// Singleton UserManager — created once per app lifetime
const userManager = new UserManager(oidcConfig);

interface AuthProviderProps {
  children: ReactNode;
}

// AuthProvider: initialises OIDC session, exposes user state + actions via context
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const initialised = useRef(false);

  useEffect(() => {
    if (initialised.current) return;
    initialised.current = true;

    // Load existing user session from memory/storage
    userManager.getUser().then((u) => {
      setUser(u);
      setIsLoading(false);
    });

    const onUserLoaded = (u: User) => setUser(u);
    // H-05: Stop SignalR on session expiry/unload to prevent stale WebSocket connections
    const onUserUnloaded = () => {
      setUser(null);
      stopSignalR().catch(() => {});
    };
    const onSilentRenewError = (err: Error) => {
      console.error('[Auth] Silent renew error:', err);
      const n = getNotificationInstance();
      n?.error({
        key: 'session-renew-failed',
        message: 'Phiên đã hết hạn',
        description: 'Không thể gia hạn phiên. Vui lòng đăng nhập lại.',
        duration: 0,
        btn: (
          <button
            onClick={() => {
              n?.destroy('session-renew-failed');
              userManager.signinRedirect();
            }}
            style={{
              cursor: 'pointer',
              background: '#ff4d4f',
              color: '#fff',
              border: 'none',
              borderRadius: 6,
              padding: '4px 16px',
              fontSize: 13,
              fontWeight: 500,
            }}
          >
            Đăng nhập lại
          </button>
        ),
      });
    };

    userManager.events.addUserLoaded(onUserLoaded);
    userManager.events.addUserUnloaded(onUserUnloaded);
    userManager.events.addSilentRenewError(onSilentRenewError);

    return () => {
      userManager.events.removeUserLoaded(onUserLoaded);
      userManager.events.removeUserUnloaded(onUserUnloaded);
      userManager.events.removeSilentRenewError(onSilentRenewError);
    };
  }, []);

  // Poll every 60s — warn user when token expires within 2 minutes.
  // Dismiss the warning automatically when token is renewed (user state updates).
  useEffect(() => {
    const POLL_MS = 60_000;
    const WARN_THRESHOLD_S = 120;

    // If token was just refreshed (expires_at far from threshold), dismiss stale warning
    if (user && !user.expired) {
      const remaining = (user.expires_at ?? 0) - Math.floor(Date.now() / 1000);
      if (remaining > WARN_THRESHOLD_S) {
        getNotificationInstance()?.destroy('session-expiring');
      }
    }

    const id = setInterval(() => {
      if (!user || user.expired) return;
      const expiresInSeconds = (user.expires_at ?? 0) - Math.floor(Date.now() / 1000);
      if (expiresInSeconds > 0 && expiresInSeconds <= WARN_THRESHOLD_S) {
        const n = getNotificationInstance();
        n?.warning({
          key: 'session-expiring',
          message: 'Phiên sắp hết hạn',
          description: 'Phiên làm việc của bạn sẽ hết hạn trong 2 phút.',
          duration: 0,
          btn: (
            <button
              onClick={() => {
                userManager.signinSilent().catch((err) =>
                  console.error('[Auth] Silent renew failed:', err)
                );
                n?.destroy('session-expiring');
              }}
              style={{
                cursor: 'pointer',
                background: '#007BFF',
                color: '#fff',
                border: 'none',
                borderRadius: 6,
                padding: '4px 16px',
                fontSize: 13,
                fontWeight: 500,
              }}
            >
              Gia hạn
            </button>
          ),
        });
      }
    }, POLL_MS);

    return () => clearInterval(id);
  }, [user]);

  const login = useCallback(async () => {
    await userManager.signinRedirect();
  }, []);

  const logout = useCallback(async () => {
    // H-05: Clean up SignalR WebSocket before redirect to prevent stale connections
    await stopSignalR();
    await userManager.signoutRedirect();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user && !user.expired,
        isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

// Export userManager for use in callback/silent-renew pages
export { userManager };
