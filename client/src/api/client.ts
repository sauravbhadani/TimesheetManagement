import axios from "axios";
import { env } from "../config/env";

export const apiClient = axios.create({
  baseURL: env.apiBaseUrl,
});

/**
 * Set once by AuthContext at startup, to whichever function knows how to get a fresh token
 * for the active auth mode (localStorage lookup for Local, MSAL acquireTokenSilent for EntraId).
 * Keeps this module ignorant of which auth mode is active.
 */
let accessTokenGetter: () => Promise<string | null> = async () => null;

export function setAccessTokenGetter(getter: () => Promise<string | null>) {
  accessTokenGetter = getter;
}

apiClient.interceptors.request.use(async (config) => {
  const token = await accessTokenGetter();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

/** Flattens a ProblemDetails/ValidationProblemDetails error response into one display string. */
export function describeApiError(error: unknown): string {
  if (axios.isAxiosError<ApiProblem>(error)) {
    const problem = error.response?.data;
    if (problem?.errors) {
      return Object.values(problem.errors).flat().join(" ");
    }
    if (problem?.detail) return problem.detail;
    if (problem?.title) return problem.title;
    if (error.response?.status === 401) return "Your session has expired. Please sign in again.";
    if (error.response?.status === 403) return "You do not have permission to do that.";
  }
  return "Something went wrong. Please try again.";
}
