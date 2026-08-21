import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { describeApiError } from "../../api/client";
import { timesheetsApi } from "../../api/timesheetsApi";
import type { TimesheetStatus, TimesheetWeekDto } from "../../api/types";
import { StatusBadge } from "../../components/StatusBadge";
import { formatWeekRange } from "../../utils/week";

const STATUS_OPTIONS: (TimesheetStatus | "All")[] = ["Submitted", "All", "Draft", "Approved", "Rejected"];

export function TeamTimesheetsPage() {
  const [status, setStatus] = useState<TimesheetStatus | "All">("Submitted");
  const [weeks, setWeeks] = useState<TimesheetWeekDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await timesheetsApi.getTeam(status === "All" ? undefined : status);
      setWeeks(data);
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setLoading(false);
    }
  }, [status]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Team Timesheets</h1>
        <select className="form-select" style={{ width: 200 }} value={status} onChange={(e) => setStatus(e.target.value as TimesheetStatus | "All")}>
          {STATUS_OPTIONS.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {loading ? (
        <div className="text-center py-5 text-muted">Loading…</div>
      ) : (
        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead className="table-light">
              <tr>
                <th>Employee</th>
                <th>Week</th>
                <th>Status</th>
                <th>Total Hours</th>
                <th>Submitted</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {weeks.length === 0 && (
                <tr>
                  <td colSpan={6} className="text-center text-muted py-4">
                    Nothing here.
                  </td>
                </tr>
              )}
              {weeks.map((w) => (
                <tr key={w.id}>
                  <td>{w.userFullName}</td>
                  <td>{formatWeekRange(w.weekStartDate, w.weekEndDate)}</td>
                  <td>
                    <StatusBadge status={w.status} />
                  </td>
                  <td>{w.totalHours}</td>
                  <td>{w.submittedAt ? new Date(w.submittedAt).toLocaleDateString() : "—"}</td>
                  <td>
                    <Link to={`/team/${w.id}`} className="btn btn-sm btn-outline-primary">
                      Review
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
