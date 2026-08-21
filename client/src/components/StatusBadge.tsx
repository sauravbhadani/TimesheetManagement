import type { TimesheetStatus } from "../api/types";

const STYLES: Record<TimesheetStatus, string> = {
  Draft: "text-bg-secondary",
  Submitted: "text-bg-warning",
  Approved: "text-bg-success",
  Rejected: "text-bg-danger",
};

export function StatusBadge({ status }: { status: TimesheetStatus }) {
  return <span className={`badge ${STYLES[status]}`}>{status}</span>;
}
