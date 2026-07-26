import type { PatientHistoryItem } from '../types';
import { STATUS_LABELS, fmtDateTime, statusClass } from '../lib/format';

interface Props {
  items: PatientHistoryItem[] | null;
  error: string | null;
}

export function PatientHistory({ items, error }: Props) {
  if (error) return <p className="field-error">{error}</p>;
  if (items === null) return <p className="muted">Loading…</p>;
  if (items.length === 0) return <p className="muted">No previous bookings for this patient.</p>;

  return (
    <ul className="history-list">
      {items.map((h, i) => (
        <li key={i}>
          <div>
            <div className="history-when">{fmtDateTime(h.startTimeUtc)}</div>
            <div className="history-who">{h.doctorName} · {h.specialtyName}</div>
          </div>
          <span className={statusClass(h.status)}>{STATUS_LABELS[h.status]}</span>
        </li>
      ))}
    </ul>
  );
}
