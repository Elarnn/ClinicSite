import type { AppointmentStatus } from '../types';

export function fmtTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

export function fmtDate(utc: string): string {
  return new Date(utc).toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
}

export function fmtDateLong(utc: string): string {
  return new Date(utc).toLocaleDateString([], {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  });
}

export function fmtDateTime(utc: string): string {
  return `${fmtDate(utc)} · ${fmtTime(utc)}`;
}

export const STATUS_LABELS: Record<AppointmentStatus, string> = {
  Scheduled: 'Scheduled',
  CheckedIn: 'Checked in',
  InProgress: 'In progress',
  Completed: 'Completed',
  NoShow: 'No-show',
  Cancelled: 'Cancelled',
};

/** CSS class for a status/slot-status badge, e.g. "badge badge-completed". */
export function statusClass(status: string): string {
  return `badge badge-${status.toLowerCase()}`;
}

/** A datetime-local input value (local wall time) → ISO UTC instant. */
export function localInputToUtcIso(localValue: string): string {
  return new Date(localValue).toISOString();
}
