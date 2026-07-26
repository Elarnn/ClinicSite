import type { PatientHistoryItem } from '../types';
import { STATUS_LABELS, fmtDateTime, statusClass } from '../lib/format';

interface Props {
  items: PatientHistoryItem[] | null;
  error: string | null;
  /** The booking the drawer is open on — marked "This visit" so it reads as an anchor, not a duplicate. */
  currentBookingId: string;
}

export function PatientHistory({ items, error, currentBookingId }: Props) {
  if (error) return <p className="field-error">{error}</p>;
  if (items === null) return <p className="muted">Loading…</p>;
  if (items.length === 0) return <p className="muted">No bookings for this patient.</p>;

  return (
    <ul className="history-list">
      {items.map((h) => (
        <li key={h.bookingId} className={h.bookingId === currentBookingId ? 'history-current' : undefined}>
          <div>
            <div className="history-when">
              {fmtDateTime(h.startTimeUtc)}
              {h.bookingId === currentBookingId && <span className="history-tag">This visit</span>}
            </div>
            <div className="history-who">{h.doctorName} · {h.specialtyName}</div>
          </div>
          <span className={statusClass(h.status)}>{STATUS_LABELS[h.status]}</span>
        </li>
      ))}
    </ul>
  );
}
