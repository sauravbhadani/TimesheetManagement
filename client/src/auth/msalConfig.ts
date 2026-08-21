import type { Configuration } from "@azure/msal-browser";
import { env } from "../config/env";

export const msalConfig: Configuration = {
  auth: {
    clientId: env.msal.clientId,
    authority: `https://login.microsoftonline.com/${env.msal.tenantId}`,
    redirectUri: env.msal.redirectUri,
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
};

export const apiTokenRequest = {
  scopes: env.msal.apiScope ? [env.msal.apiScope] : [],
};
