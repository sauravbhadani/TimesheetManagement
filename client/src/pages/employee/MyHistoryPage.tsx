import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { describeApiError } from "../../api/client";
import { timesheetsApi } from "../../api/timesheetsApi";
import type { TimesheetWeekDto } from "../../api/types";
import { StatusBadge } from "../../components/StatusBadge";
import { formatWeekRange } from "../../utils/week";

export function MyHistoryPage() {
  const [weeks, setWeeks] = useState<TimesheetWeekDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    timesheetsApi
      .getMyHistory()
      .then(setWeeks)
      .catch((err) => setError(describeApiError(err)))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="h4 mb-3">My History</h1>
      {error && <div className="alert alert-danger">{error}</div>}
      {loading ? (
        <div className="text-center py-5 text-muted">Loading…</div>
      ) : (
        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead className="table-light">
              <tr>
                <th>Week</th>
                <th>Status</th>
                <th>Total Hours</th>
                <th>Submitted</th>
                <th>Approved / Rejected</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {weeks.length === 0 && (
                <tr>
                  <td colSpan={6} className="text-center text-muted py-4">
                    No timesheets yet.
                  </td>
                </tr>
              )}
              {weeks.map((w) => (
                <tr key={w.id}>
                  <td>{formatWeekRange(w.weekStartDate, w.weekEndDate)}</td>
                  <td>
                    <StatusBadge status={w.status} />
                  </td>
                  <td>{w.totalHours}</td>
                  <td>{w.submittedAt ? new Date(w.submittedAt).toLocaleDateString() : "—"}</td>
                  <td>
                    {w.status === "Approved" && w.approvedAt ? new Date(w.approvedAt).toLocaleDateString() : null}
                    {w.status === "Rejected" && (
                      <span className="text-danger small">{w.rejectionComment}</span>
                    )}
                    {w.status === "Draft" && "—"}
                  </td>
                  <td>
                    <Link to={`/my-timesheet?week=${w.weekStartDate}`} className="btn btn-sm btn-outline-primary">
                      View
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
