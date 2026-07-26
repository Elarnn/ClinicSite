import type { ScheduleItem } from '../types';
import { STATUS_LABELS, fmtTime, statusClass } from '../lib/format';

interface Props {
  items: ScheduleItem[];
  onOpen: (item: ScheduleItem) => void;
  onBlock: (slotId: string) => void;
  onUnblock: (slotId: string) => void;
  busySlotId: string | null;
}

function dayKey(utc: string): string {
  const d = new Date(utc);
  return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

function dayLabel(utc: string): string {
  return new Date(utc).toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' });
}

function isPast(utc: string): boolean {
  return new Date(utc).getTime() <= Date.now();
}

export function DoctorCalendarView({ items, onOpen, onBlock, onUnblock, busySlotId }: Props) {
  const groups = new Map<string, ScheduleItem[]>();
  for (const it of items) {
    const key = dayKey(it.startTimeUtc);
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key)!.push(it);
  }

  const days = Array.from(groups.entries());
  if (days.length === 0) return <p className="muted">No slots this week.</p>;

  return (
    <div className="week-grid">
      {days.map(([key, dayItems]) => (
        <div key={key} className="week-col">
          <div className="week-col-head">{dayLabel(dayItems[0].startTimeUtc)}</div>
          {dayItems.map((it) => {
            const booked = it.bookingId !== null;
            const past = isPast(it.startTimeUtc);
            return (
              <div key={it.slotId} className={`week-item ${booked ? 'booked' : it.slotStatus.toLowerCase()}`}>
                <div className="week-item-top" onClick={() => booked && onOpen(it)}>
                  <span className="week-time">{fmtTime(it.startTimeUtc)}</span>
                  <span className="week-who">
                    {booked ? it.patientName : it.slotStatus === 'Blocked' ? 'Blocked' : 'Free'}
                  </span>
                </div>
                {booked && it.status && (
                  <span className={statusClass(it.status)}>{STATUS_LABELS[it.status]}</span>
                )}
                {!booked && !past && it.slotStatus === 'Free' && (
                  <button
                    className="mini-btn"
                    title="Block only this slot"
                    disabled={busySlotId === it.slotId}
                    onClick={() => onBlock(it.slotId)}
                  >
                    Block
                  </button>
                )}
                {!booked && !past && it.slotStatus === 'Blocked' && (
                  <button
                    className="mini-btn"
                    title="Unblock only this slot"
                    disabled={busySlotId === it.slotId}
                    onClick={() => onUnblock(it.slotId)}
                  >
                    Unblock
                  </button>
                )}
              </div>
            );
          })}
        </div>
      ))}
    </div>
  );
}
