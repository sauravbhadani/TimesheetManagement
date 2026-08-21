import { PublicClientApplication } from "@azure/msal-browser";
import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { authApi, type CurrentUser, type LocalUserOption } from "../api/authApi";
import { setAccessTokenGetter } from "../api/client";
import { env } from "../config/env";
import { apiTokenRequest, msalConfig } from "./msalConfig";

const LOCAL_TOKEN_KEY = "timesheet.local.token";

type AuthStatus = "loading" | "signed-out" | "signed-in";

interface AuthContextValue {
  status: AuthStatus;
  user: CurrentUser | null;
  authProvider: "Local" | "EntraId";
  localUsers: LocalUserOption[];
  loginLocal: (userId: string) => Promise<void>;
  loginEntra: () => Promise<void>;
  logout: () => Promise<void>;
  hasRole: (...roles: CurrentUser["role"][]) => boolean;
  error: string | null;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

// Only constructed when Auth:Provider = EntraId — never touched in Local mode.
const msalInstance = env.authProvider === "EntraId" ? new PublicClientApplication(msalConfig) : null;

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [localUsers, setLocalUsers] = useState<LocalUserOption[]>([]);
  const [error, setError] = useState<string | null>(null);

  const hydrateFromToken = useCallback(async () => {
    try {
      const me = await authApi.me();
      setUser(me);
      setStatus("signed-in");
    } catch {
      setUser(null);
      setStatus("signed-out");
    }
  }, []);

  useEffect(() => {
    let cancelled = false;

    async function initLocal() {
      setAccessTokenGetter(async () => sessionStorage.getItem(LOCAL_TOKEN_KEY));

      try {
        const users = await authApi.getLocalUsers();
        if (!cancelled) setLocalUsers(users);
      } catch {
        // Local login endpoint not reachable yet — dropdown just stays empty until retried.
      }

      if (sessionStorage.getItem(LOCAL_TOKEN_KEY)) {
        await hydrateFromToken();
      } else if (!cancelled) {
        setStatus("signed-out");
      }
    }

    async function initEntra() {
      if (!msalInstance) return;
      await msalInstance.initialize();
      const redirectResult = await msalInstance.handleRedirectPromise();
      const account = redirectResult?.account ?? msalInstance.getAllAccounts()[0];

      if (account) {
        msalInstance.setActiveAccount(account);
      }

      setAccessTokenGetter(async () => {
        const active = msalInstance.getActiveAccount();
        if (!active) return null;
        try {
          const result = await msalInstance.acquireTokenSilent({ ...apiTokenRequest, account: active });
          return result.accessToken;
        } catch {
          return null;
        }
      });

      if (account) {
        await hydrateFromToken();
      } else if (!cancelled) {
        setStatus("signed-out");
      }
    }

    if (env.authProvider === "EntraId") {
      void initEntra();
    } else {
      void initLocal();
    }

    return () => {
      cancelled = true;
    };
  }, [hydrateFromToken]);

  const loginLocal = useCallback(async (userId: string) => {
    setError(null);
    try {
      const { token } = await authApi.localLogin(userId);
      sessionStorage.setItem(LOCAL_TOKEN_KEY, token);
      await hydrateFromToken();
    } catch {
      setError("Could not sign in as that user.");
    }
  }, [hydrateFromToken]);

  const loginEntra = useCallback(async () => {
    if (!msalInstance) return;
    setError(null);
    try {
      await msalInstance.loginRedirect({ scopes: apiTokenRequest.scopes });
    } catch {
      setError("Sign-in failed. Please try again.");
    }
  }, []);

  const logout = useCallback(async () => {
    if (env.authProvider === "EntraId" && msalInstance) {
      await msalInstance.logoutRedirect();
      return;
    }
    sessionStorage.removeItem(LOCAL_TOKEN_KEY);
    setUser(null);
    setStatus("signed-out");
  }, []);

  const hasRole = useCallback((...roles: CurrentUser["role"][]) => !!user && roles.includes(user.role), [user]);

  const value = useMemo<AuthContextValue>(
    () => ({ status, user, authProvider: env.authProvider, localUsers, loginLocal, loginEntra, logout, hasRole, error }),
    [status, user, localUsers, loginLocal, loginEntra, logout, hasRole, error],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
