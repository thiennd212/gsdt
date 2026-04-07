import { WebStorageStateStore } from 'oidc-client-ts';

// OIDC configuration for gsdt-spa-dev — PKCE flow, no client secret
// sessionStorage: tokens survive HMR/tab refresh but cleared on tab close (secure enough for admin app)
export const oidcConfig = {
  authority: import.meta.env.VITE_OIDC_AUTHORITY ?? 'http://localhost:5002',
  client_id: 'gsdt-spa-dev',
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile email roles api offline_access',
  automaticSilentRenew: true,
  silent_redirect_uri: `${window.location.origin}/silent-renew.html`,
  userStore: new WebStorageStateStore({ store: sessionStorage }),
} as const;
