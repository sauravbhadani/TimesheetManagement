import { useState } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

export function LoginPage() {
  const { status, authProvider, localUsers, loginLocal, loginEntra, error } = useAuth();
  const [selectedUserId, setSelectedUserId] = useState("");
  const [submitting, setSubmitting] = useState(false);

  if (status === "signed-in") {
    return <Navigate to="/" replace />;
  }

  async function handleLocalSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedUserId) return;
    setSubmitting(true);
    await loginLocal(selectedUserId);
    setSubmitting(false);
  }

  return (
    <div className="d-flex align-items-center justify-content-center vh-100 bg-body-tertiary">
      <div className="card shadow-sm" style={{ width: "22rem" }}>
        <div className="card-body p-4">
          <h1 className="h4 mb-1">Timesheet</h1>
          <p className="text-muted small mb-4">
            {authProvider === "Local" ? "Development sign-in" : "Sign in with your work account"}
          </p>

          {error && <div className="alert alert-danger py-2 small">{error}</div>}

          {authProvider === "Local" ? (
            <form onSubmit={handleLocalSubmit}>
              <label htmlFor="local-user" className="form-label small text-muted">
                Sign in as
              </label>
              <select
                id="local-user"
                className="form-select mb-3"
                value={selectedUserId}
                onChange={(e) => setSelectedUserId(e.target.value)}
                required
              >
                <option value="" disabled>
                  Choose a test user…
                </option>
                {localUsers.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.fullName} — {u.role}
                  </option>
                ))}
              </select>
              <button type="submit" className="btn btn-primary w-100" disabled={!selectedUserId || submitting}>
                {submitting ? "Signing in…" : "Sign in"}
              </button>
            </form>
          ) : (
            <button type="button" className="btn btn-primary w-100" onClick={() => void loginEntra()}>
              Sign in with Microsoft
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
