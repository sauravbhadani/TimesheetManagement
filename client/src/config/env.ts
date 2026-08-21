export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7059",
  authProvider: (import.meta.env.VITE_AUTH_PROVIDER ?? "Local") as "Local" | "EntraId",
  msal: {
    clientId: import.meta.env.VITE_MSAL_CLIENT_ID ?? "",
    tenantId: import.meta.env.VITE_MSAL_TENANT_ID ?? "",
    apiScope: import.meta.env.VITE_MSAL_API_SCOPE ?? "",
    redirectUri: import.meta.env.VITE_MSAL_REDIRECT_URI ?? window.location.origin,
  },
};
