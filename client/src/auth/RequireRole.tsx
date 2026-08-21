import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import type { UserRole } from "../api/types";
import { useAuth } from "./AuthContext";

export function RequireRole({ roles, children }: { roles: UserRole[]; children: ReactNode }) {
  const { status, hasRole } = useAuth();

  if (status === "loading") {
    return (
      <div className="d-flex justify-content-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading…</span>
        </div>
      </div>
    );
  }

  if (status === "signed-out") {
    return <Navigate to="/login" replace />;
  }

  if (!hasRole(...roles)) {
    return (
      <div className="alert alert-warning m-4" role="alert">
        You do not have access to this page.
      </div>
    );
  }

  return <>{children}</>;
}
