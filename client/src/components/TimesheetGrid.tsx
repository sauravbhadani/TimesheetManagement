import type { TaskClassification } from "../api/types";

export interface GridRow {
  projectId: string;
  projectTaskId: string;
  projectName: string;
  projectTaskName: string;
  classification: TaskClassification;
  isBillable: boolean;
  mon: number;
  tue: number;
  wed: number;
  thu: number;
  fri: number;
  sat: number;
  sun: number;
  notes: string;
}

export const DAY_KEYS = ["mon", "tue", "wed", "thu", "fri", "sat", "sun"] as const;
export type DayKey = (typeof DAY_KEYS)[number];
const DAY_LABELS: Record<DayKey, string> = { mon: "Mon", tue: "Tue", wed: "Wed", thu: "Thu", fri: "Fri", sat: "Sat", sun: "Sun" };

export function rowTotal(row: GridRow): number {
  return DAY_KEYS.reduce((sum, day) => sum + (row[day] || 0), 0);
}

interface TimesheetGridProps {
  rows: GridRow[];
  readOnly: boolean;
  onHourChange?: (rowIndex: number, day: DayKey, value: number) => void;
  onNotesChange?: (rowIndex: number, value: string) => void;
  onRemoveRow?: (rowIndex: number) => void;
}

export function TimesheetGrid({ rows, readOnly, onHourChange, onNotesChange, onRemoveRow }: TimesheetGridProps) {
  const dayTotals = DAY_KEYS.map((day) => rows.reduce((sum, r) => sum + (r[day] || 0), 0));
  const grandTotal = dayTotals.reduce((a, b) => a + b, 0);

  return (
    <div className="table-responsive">
      <table className="table table-bordered align-middle mb-0">
        <thead className="table-light">
          <tr>
            <th style={{ minWidth: 220 }}>Project / Task</th>
            {DAY_KEYS.map((day) => (
              <th key={day} className="text-center" style={{ width: 76 }}>
                {DAY_LABELS[day]}
              </th>
            ))}
            <th className="text-center" style={{ width: 90 }}>
              Total
            </th>
            <th style={{ minWidth: 160 }}>Notes</th>
            {!readOnly && <th style={{ width: 40 }} />}
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 && (
            <tr>
              <td colSpan={11} className="text-center text-muted py-4">
                No projects added yet.
              </td>
            </tr>
          )}
          {rows.map((row, index) => (
            <tr key={`${row.projectId}-${row.projectTaskId}`}>
              <td>
                <div className="fw-semibold">{row.projectName}</div>
                <div className="small text-muted">
                  {row.projectTaskName}{" "}
                  <span className="badge text-bg-light text-dark border">{row.classification}</span>{" "}
                  {row.isBillable && <span className="badge text-bg-info-subtle text-info-emphasis border">Billable</span>}
                </div>
              </td>
              {DAY_KEYS.map((day) => (
                <td key={day} className="text-center p-1">
                  {readOnly ? (
                    <span>{row[day] || 0}</span>
                  ) : (
                    <input
                      type="number"
                      min={0}
                      step={0.5}
                      className="form-control form-control-sm text-center"
                      value={row[day] === 0 ? "" : row[day]}
                      placeholder="0"
                      onChange={(e) => onHourChange?.(index, day, Number(e.target.value) || 0)}
                    />
                  )}
                </td>
              ))}
              <td className="text-center fw-semibold">{rowTotal(row)}</td>
              <td className="p-1">
                {readOnly ? (
                  <span className="small text-muted">{row.notes}</span>
                ) : (
                  <input
                    type="text"
                    className="form-control form-control-sm"
                    value={row.notes}
                    onChange={(e) => onNotesChange?.(index, e.target.value)}
                  />
                )}
              </td>
              {!readOnly && (
                <td className="text-center p-1">
                  <button
                    type="button"
                    className="btn btn-sm btn-link text-danger p-0"
                    aria-label="Remove row"
                    onClick={() => onRemoveRow?.(index)}
                  >
                    ✕
                  </button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr className="table-light fw-semibold">
            <td>Total</td>
            {dayTotals.map((total, i) => (
              <td key={DAY_KEYS[i]} className="text-center">
                {total}
              </td>
            ))}
            <td className="text-center">{grandTotal}</td>
            <td />
            {!readOnly && <td />}
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
