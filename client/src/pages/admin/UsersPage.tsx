import { useCallback, useEffect, useState } from "react";
import { describeApiError } from "../../api/client";
import { usersApi } from "../../api/usersApi";
import type { UserDto, UserRole } from "../../api/types";

const ROLES: UserRole[] = ["Employee", "Manager", "Admin"];

export function UsersPage() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      setUsers(await usersApi.getAll());
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function handleRoleChange(user: UserDto, role: UserRole) {
    setError(null);
    try {
      await usersApi.updateRole(user.id, role);
      await load();
    } catch (err) {
      setError(describeApiError(err));
    }
  }

  async function handleManagerChange(user: UserDto, managerId: string) {
    setError(null);
    try {
      await usersApi.updateManager(user.id, managerId || null);
      await load();
    } catch (err) {
      setError(describeApiError(err));
    }
  }

  const managers = users.filter((u) => u.role === "Manager" || u.role === "Admin");

  return (
    <div>
      <h1 className="h4 mb-3">Users</h1>
      {error && <div className="alert alert-danger">{error}</div>}

      {loading ? (
        <div className="text-muted">Loading…</div>
      ) : (
        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead className="table-light">
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th style={{ width: 180 }}>Role</th>
                <th style={{ width: 220 }}>Manager</th>
                <th>Active</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id}>
                  <td>{u.fullName}</td>
                  <td className="text-muted small">{u.email}</td>
                  <td>
                    <select className="form-select form-select-sm" value={u.role} onChange={(e) => void handleRoleChange(u, e.target.value as UserRole)}>
                      {ROLES.map((r) => (
                        <option key={r} value={r}>
                          {r}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <select
                      className="form-select form-select-sm"
                      value={u.managerId ?? ""}
                      onChange={(e) => void handleManagerChange(u, e.target.value)}
                    >
                      <option value="">— None —</option>
                      {managers
                        .filter((m) => m.id !== u.id)
                        .map((m) => (
                          <option key={m.id} value={m.id}>
                            {m.fullName}
                          </option>
                        ))}
                    </select>
                  </td>
                  <td>{u.isActive ? <span className="badge text-bg-success">Active</span> : <span className="badge text-bg-secondary">Inactive</span>}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
