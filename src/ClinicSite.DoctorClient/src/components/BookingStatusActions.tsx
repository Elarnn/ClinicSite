import type { AppointmentStatus } from '../types';

const ACTIONS: { label: string; status: AppointmentStatus }[] = [
  { label: 'Checked in', status: 'CheckedIn' },
  { label: 'Start', status: 'InProgress' },
  { label: 'Complete', status: 'Completed' },
  { label: 'No-show', status: 'NoShow' },
  { label: 'Cancel', status: 'Cancelled' },
];

interface Props {
  current: AppointmentStatus;
  busy: boolean;
  onChange: (status: AppointmentStatus) => void;
}

export function BookingStatusActions({ current, busy, onChange }: Props) {
  return (
    <div className="status-actions">
      {ACTIONS.map((a) => (
        <button
          key={a.status}
          className={`status-btn ${a.status.toLowerCase()}${current === a.status ? ' active' : ''}`}
          disabled={busy || current === a.status}
          onClick={() => onChange(a.status)}
        >
          {a.label}
        </button>
      ))}
    </div>
  );
}
