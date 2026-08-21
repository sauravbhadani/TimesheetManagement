import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";

export interface BrandingConfig {
  companyName: string;
  logoDataUrl: string | null;
  primaryColor: string;
  secondaryColor: string;
}

const STORAGE_KEY = "timesheet.branding";

const DEFAULT_BRANDING: BrandingConfig = {
  companyName: "Timesheet",
  logoDataUrl: null,
  primaryColor: "#0d6efd", // Bootstrap default primary
  secondaryColor: "#6c757d", // Bootstrap default secondary
};

function loadBranding(): BrandingConfig {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return DEFAULT_BRANDING;
    return { ...DEFAULT_BRANDING, ...(JSON.parse(raw) as Partial<BrandingConfig>) };
  } catch {
    return DEFAULT_BRANDING;
  }
}

function applyBranding(branding: BrandingConfig) {
  const root = document.documentElement.style;
  root.setProperty("--bs-primary", branding.primaryColor);
  root.setProperty("--bs-primary-rgb", hexToRgb(branding.primaryColor));
  root.setProperty("--bs-secondary", branding.secondaryColor);
  root.setProperty("--bs-secondary-rgb", hexToRgb(branding.secondaryColor));
  root.setProperty("--brand-primary", branding.primaryColor);
  root.setProperty("--brand-secondary", branding.secondaryColor);

  document.title = branding.companyName;

  const favicon = document.querySelector<HTMLLinkElement>("link[rel~='icon']");
  if (favicon && branding.logoDataUrl) {
    favicon.href = branding.logoDataUrl;
  }
}

function hexToRgb(hex: string): string {
  const clean = hex.replace("#", "");
  const bigint = parseInt(clean.length === 3 ? clean.split("").map((c) => c + c).join("") : clean, 16);
  if (Number.isNaN(bigint)) return "13,110,253";
  const r = (bigint >> 16) & 255;
  const g = (bigint >> 8) & 255;
  const b = bigint & 255;
  return `${r},${g},${b}`;
}

interface BrandingContextValue {
  branding: BrandingConfig;
  updateBranding: (branding: BrandingConfig) => void;
  resetBranding: () => void;
}

const BrandingContext = createContext<BrandingContextValue | undefined>(undefined);

export function BrandingProvider({ children }: { children: ReactNode }) {
  const [branding, setBranding] = useState<BrandingConfig>(loadBranding);

  useEffect(() => {
    applyBranding(branding);
  }, [branding]);

  const updateBranding = useCallback((next: BrandingConfig) => {
    setBranding(next);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  }, []);

  const resetBranding = useCallback(() => {
    setBranding(DEFAULT_BRANDING);
    localStorage.removeItem(STORAGE_KEY);
  }, []);

  const value = useMemo(() => ({ branding, updateBranding, resetBranding }), [branding, updateBranding, resetBranding]);

  return <BrandingContext.Provider value={value}>{children}</BrandingContext.Provider>;
}

export function useBranding(): BrandingContextValue {
  const ctx = useContext(BrandingContext);
  if (!ctx) throw new Error("useBranding must be used within a BrandingProvider");
  return ctx;
}
