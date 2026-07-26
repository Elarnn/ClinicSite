import type { ScheduleItem } from '../types';
import { STATUS_LABELS, fmtTime, statusClass } from '../lib/format';

interface Props {
  items: ScheduleItem[];
  highlightBookingId?: string | null;
  onOpen: (item: ScheduleItem) => void;
  onBlock: (slotId: string) => void;
  onUnblock: (slotId: string) => void;
  busySlotId: string | null;
}

export function DoctorSchedule({ items, highlightBookingId, onOpen, onBlock, onUnblock, busySlotId }: Props) {
  if (items.length === 0) return <p className="muted">No slots for this day.</p>;

  return (
    <div className="schedule">
      {items.map((it) => {
        const booked = it.bookingId !== null;
        // In the Today view "blocked" means the whole time-of-day is blocked on every day.
        const blocked = !booked && (it.recurringBlocked || it.slotStatus === 'Blocked');
        const isNext = Boolean(highlightBookingId) && it.bookingId === highlightBookingId;
        const kind = booked ? 'booked' : blocked ? 'blocked' : it.slotStatus.toLowerCase();

        return (
          <div key={it.slotId} className={`schedule-row ${kind}${isNext ? ' next' : ''}`}>
            <div className="schedule-time">
              {fmtTime(it.startTimeUtc)}
              <span>{fmtTime(it.endTimeUtc)}</span>
            </div>

            <div className="schedule-main">
              {booked ? (
                <button className="schedule-open" onClick={() => onOpen(it)}>
                  <span className="schedule-patient">{it.patientName}</span>
                  <span className="schedule-email">{it.patientEmail}</span>
                </button>
              ) : (
                <span className="schedule-free-label">{blocked ? 'Blocked (every day)' : 'Free window'}</span>
              )}
            </div>

            <div className="schedule-side">
              {isNext && <span className="next-badge">Next</span>}
              {booked && it.status && <span className={statusClass(it.status)}>{STATUS_LABELS[it.status]}</span>}
              {!booked && blocked && (
                <button
                  className="mini-btn"
                  title="Unblock this time on every day"
                  disabled={busySlotId === it.slotId}
                  onClick={() => onUnblock(it.slotId)}
                >
                  Unblock daily
                </button>
              )}
              {!booked && !blocked && (
                <button
                  className="mini-btn"
                  title="Block this time on every day"
                  disabled={busySlotId === it.slotId}
                  onClick={() => onBlock(it.slotId)}
                >
                  Block daily
                </button>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
