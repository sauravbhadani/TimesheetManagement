import { useCallback, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { describeApiError } from "../../api/client";
import { projectsApi } from "../../api/projectsApi";
import { timesheetsApi } from "../../api/timesheetsApi";
import type { ProjectDto, ProjectTaskDto, SaveTimesheetEntryRequest, TimesheetWeekDto } from "../../api/types";
import { TimesheetGrid, type DayKey, type GridRow } from "../../components/TimesheetGrid";
import { StatusBadge } from "../../components/StatusBadge";
import { addDays, formatWeekRange, mostRecentMonday, toDateOnlyString } from "../../utils/week";

function entriesToRows(week: TimesheetWeekDto | null): GridRow[] {
  if (!week) return [];
  return week.entries.map((e) => ({
    projectId: e.projectId,
    projectTaskId: e.projectTaskId,
    projectName: e.projectName,
    projectTaskName: e.projectTaskName,
    classification: e.classification,
    isBillable: e.isBillable,
    mon: e.monHours,
    tue: e.tueHours,
    wed: e.wedHours,
    thu: e.thuHours,
    fri: e.friHours,
    sat: e.satHours,
    sun: e.sunHours,
    notes: e.notes ?? "",
  }));
}

export function MyTimesheetPage() {
  const [searchParams] = useSearchParams();
  const [weekStart, setWeekStart] = useState(() => {
    const requested = searchParams.get("week");
    return requested ? mostRecentMonday(new Date(requested)) : mostRecentMonday(new Date());
  });
  const [week, setWeek] = useState<TimesheetWeekDto | null>(null);
  const [rows, setRows] = useState<GridRow[]>([]);
  const [availableTasks, setAvailableTasks] = useState<ProjectTaskDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [warnings, setWarnings] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [addSelection, setAddSelection] = useState("");

  const weekStartStr = toDateOnlyString(weekStart);
  const isLocked = week?.status === "Submitted" || week?.status === "Approved";

  const loadWeek = useCallback(async () => {
    setLoading(true);
    setError(null);
    setWarnings([]);
    try {
      const data = await timesheetsApi.getMine(weekStartStr);
      setWeek(data);
      setRows(entriesToRows(data));
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setLoading(false);
    }
  }, [weekStartStr]);

  useEffect(() => {
    void loadWeek();
  }, [loadWeek]);

  useEffect(() => {
    async function loadTaskOptions() {
      const projects: ProjectDto[] = await projectsApi.getAll();
      const taskLists = await Promise.all(projects.map((p) => projectsApi.getTasks(p.id)));
      setAvailableTasks(taskLists.flat());
    }
    void loadTaskOptions();
  }, []);

  const addableOptions = useMemo(
    () => availableTasks.filter((t) => !rows.some((r) => r.projectId === t.projectId && r.projectTaskId === t.id)),
    [availableTasks, rows],
  );

  function handleHourChange(rowIndex: number, day: DayKey, value: number) {
    setRows((prev) => prev.map((r, i) => (i === rowIndex ? { ...r, [day]: value } : r)));
  }

  function handleNotesChange(rowIndex: number, value: string) {
    setRows((prev) => prev.map((r, i) => (i === rowIndex ? { ...r, notes: value } : r)));
  }

  function handleRemoveRow(rowIndex: number) {
    setRows((prev) => prev.filter((_, i) => i !== rowIndex));
  }

  function handleAddRow(taskId: string) {
    const task = availableTasks.find((t) => t.id === taskId);
    if (!task) return;
    const newRow: GridRow = {
      projectId: task.projectId,
      projectTaskId: task.id,
      projectName: task.projectName,
      projectTaskName: task.name,
      classification: task.classification,
      isBillable: task.isBillable,
      mon: 0, tue: 0, wed: 0, thu: 0, fri: 0, sat: 0, sun: 0,
      notes: "",
    };
    setRows((prev) => [...prev, newRow]);
    setAddSelection("");
  }

  // Persists whatever is currently on screen. Shared by the Save Draft button and by Submit,
  // which must save current edits first — otherwise a click straight to Submit would silently
  // submit the last-saved server state instead of what the employee is looking at.
  async function persistDraft() {
    const entries: SaveTimesheetEntryRequest[] = rows.map((r) => ({
      projectId: r.projectId,
      projectTaskId: r.projectTaskId,
      monHours: r.mon, tueHours: r.tue, wedHours: r.wed, thuHours: r.thu,
      friHours: r.fri, satHours: r.sat, sunHours: r.sun,
      notes: r.notes || null,
    }));
    const result = await timesheetsApi.saveDraft({ weekStartDate: weekStartStr, entries });
    setWeek(result.week);
    setRows(entriesToRows(result.week));
    setWarnings(result.warnings);
    return result.week;
  }

  async function handleSaveDraft() {
    setSaving(true);
    setError(null);
    try {
      await persistDraft();
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setSaving(false);
    }
  }

  async function handleSubmit() {
    setSubmitting(true);
    setError(null);
    try {
      const savedWeek = await persistDraft();
      const submitted = await timesheetsApi.submit(savedWeek.id);
      setWeek(submitted);
      setRows(entriesToRows(submitted));
    } catch (err) {
      setError(describeApiError(err));
    } finally {
      setSubmitting(false);
    }
  }

  const weekEndStr = toDateOnlyString(addDays(weekStart, 6));

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <div>
          <h1 className="h4 mb-0">My Timesheet</h1>
          <div className="d-flex align-items-center gap-2 mt-1">
            <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setWeekStart((d) => addDays(d, -7))}>
              ‹ Prev
            </button>
            <span className="fw-semibold">{formatWeekRange(weekStartStr, weekEndStr)}</span>
            <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setWeekStart((d) => addDays(d, 7))}>
              Next ›
            </button>
            {week && <StatusBadge status={week.status} />}
          </div>
        </div>

        {!isLocked && (
          <div className="d-flex gap-2">
            <button type="button" className="btn btn-outline-primary" disabled={saving || loading} onClick={() => void handleSaveDraft()}>
              {saving ? "Saving…" : "Save Draft"}
            </button>
            <button
              type="button"
              className="btn btn-primary"
              disabled={submitting || loading || rows.length === 0}
              onClick={() => void handleSubmit()}
            >
              {submitting ? "Submitting…" : "Submit"}
            </button>
          </div>
        )}
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {week?.status === "Rejected" && week.rejectionComment && (
        <div className="alert alert-danger">
          <strong>Rejected:</strong> {week.rejectionComment}
        </div>
      )}
      {warnings.length > 0 && (
        <div className="alert alert-warning">
          {warnings.map((w) => (
            <div key={w}>{w}</div>
          ))}
        </div>
      )}
      {isLocked && (
        <div className="alert alert-info">
          This timesheet is {week?.status.toLowerCase()} and read-only.
        </div>
      )}

      {loading ? (
        <div className="text-center py-5 text-muted">Loading…</div>
      ) : (
        <>
          <TimesheetGrid
            rows={rows}
            readOnly={!!isLocked}
            onHourChange={handleHourChange}
            onNotesChange={handleNotesChange}
            onRemoveRow={handleRemoveRow}
          />

          {!isLocked && (
            <div className="mt-3" style={{ maxWidth: 420 }}>
              <label className="form-label small text-muted">Add a project / task</label>
              <select
                className="form-select"
                value={addSelection}
                onChange={(e) => {
                  setAddSelection(e.target.value);
                  handleAddRow(e.target.value);
                }}
              >
                <option value="">Select project / task…</option>
                {addableOptions.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.projectName} — {t.name}
                  </option>
                ))}
              </select>
            </div>
          )}
        </>
      )}
    </div>
  );
}
