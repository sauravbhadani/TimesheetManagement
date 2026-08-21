import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { describeApiError } from "../../api/client";
import { useAuth } from "../../auth/AuthContext";
import { timesheetsApi } from "../../api/timesheetsApi";
import type { TimesheetWeekDto } from "../../api/types";
import { StatusBadge } from "../../components/StatusBadge";
import { TimesheetGrid, type GridRow } from "../../components/TimesheetGrid";
import { formatWeekRange } from "../../utils/week";

function toRows(week: TimesheetWeekDto): GridRow[] {
  return week.entries.map((e) => ({
    projectId: e.projectId,
    projectTaskId: e.projectTaskId,
    projectName: e.projectName,
    projectTaskName: e.projectTaskName,
    classification: e.classification,
    isBillable: e.isBillable,
    mon: e.monHours, tue: e.tueHours, wed: e.wedHours, thu: e.thuHours,
    fri: e.friHours, sat: e.satHours, sun: e.sunHours,
    notes: e.notes ?? "",
  }));
}

export function ReviewTimesheetPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const canDecide = hasRole("Manager");
  const [week, setWeek] = useState<TimesheetWeekDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [comment, setComment] = useState("");
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      setWeek(await timesheetsApi.getById(id));
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    void load();
  }, [load]);

  async function handleApprove() {
    if (!week) return;
    setBusy(true);
    setError(null);
    try {
      setWeek(await timesheetsApi.approve(week.id));
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setBusy(false);
    }
  }

  async function handleReject() {
    if (!week || !comment.trim()) return;
    setBusy(true);
    setError(null);
    try {
      setWeek(await timesheetsApi.reject(week.id, { comment: comment.trim() }));
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <div className="text-center py-5 text-muted">Loading…</div>;
  if (error && !week) return <div className="alert alert-danger">{error}</div>;
  if (!week) return null;

  return (
    <div>
      <button type="button" className="btn btn-sm btn-link ps-0 mb-2" onClick={() => navigate(-1)}>
        ‹ Back
      </button>

      <div className="d-flex justify-content-between align-items-start mb-3">
        <div>
          <h1 className="h4 mb-1">{week.userFullName}</h1>
          <div className="d-flex align-items-center gap-2">
            <span className="text-muted">{formatWeekRange(week.weekStartDate, week.weekEndDate)}</span>
            <StatusBadge status={week.status} />
          </div>
        </div>
        <div className="text-end">
          <div className="small text-muted">Total Hours</div>
          <div className="h4 mb-0">{week.totalHours}</div>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {week.status === "Rejected" && week.rejectionComment && (
        <div className="alert alert-secondary">
          <strong>Previously rejected:</strong> {week.rejectionComment}
        </div>
      )}

      <TimesheetGrid rows={toRows(week)} readOnly />

      {week.status === "Submitted" && canDecide ? (
        <div className="card mt-4">
          <div className="card-body">
            <h2 className="h6">Decision</h2>
            <div className="row g-3 align-items-end">
              <div className="col-md-8">
                <label className="form-label small text-muted">Comment (required to reject)</label>
                <textarea
                  className="form-control"
                  rows={2}
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="Explain what needs to change…"
                />
              </div>
              <div className="col-md-4 d-flex gap-2">
                <button type="button" className="btn btn-success flex-fill" disabled={busy} onClick={() => void handleApprove()}>
                  Approve
                </button>
                <button
                  type="button"
                  className="btn btn-danger flex-fill"
                  disabled={busy || !comment.trim()}
                  onClick={() => void handleReject()}
                >
                  Reject
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : (
        <div className="alert alert-info mt-3">
          {week.status === "Submitted"
            ? "This timesheet is awaiting the employee's manager."
            : `This timesheet is ${week.status.toLowerCase()} and no longer awaiting action.`}{" "}
          <Link to="/team">Back</Link>.
        </div>
      )}
    </div>
  );
}
