import { useEffect, useMemo, useState } from 'react';
import { getFreeSlots, reserveSlot } from '../api/slots';
import type { AppointmentSlotDto, DoctorDto, ReserveSlotResultDto, SpecialtyDto } from '../types';
import { Spinner } from './Spinner';
import { ErrorMessage } from './ErrorMessage';

function localDateKey(utc: string): string {
  const d = new Date(utc);
  return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

function formatDateLabel(utc: string): string {
  const d = new Date(utc);
  const today = new Date();
  const tomorrow = new Date(today);
  tomorrow.setDate(today.getDate() + 1);

  if (d.toDateString() === today.toDateString()) return 'Today';
  if (d.toDateString() === tomorrow.toDateString()) return 'Tomorrow';
  return d.toLocaleDateString([], { weekday: 'long', month: 'short', day: 'numeric' });
}

function formatTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function groupByDate(slots: AppointmentSlotDto[]): [string, AppointmentSlotDto[]][] {
  const map = new Map<string, AppointmentSlotDto[]>();
  for (const slot of slots) {
    const key = localDateKey(slot.startTimeUtc);
    if (!map.has(key)) map.set(key, []);
    map.get(key)!.push(slot);
  }
  return Array.from(map.entries());
}

interface Props {
  specialty: SpecialtyDto;
  doctor: DoctorDto;
  unavailableSlots: AppointmentSlotDto[];
  onSelect: (slot: AppointmentSlotDto, reservation: ReserveSlotResultDto) => void;
}

export function SlotStep({ specialty, doctor, unavailableSlots, onSelect }: Props) {
  const [freeSlots, setFreeSlots] = useState<AppointmentSlotDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reservingId, setReservingId] = useState<string | null>(null);
  const [reserveError, setReserveError] = useState<string | null>(null);
  const [localUnavailableIds, setLocalUnavailableIds] = useState<Set<string>>(
    () => new Set(unavailableSlots.map((s) => s.slotId)),
  );

  const load = () => {
    setLoading(true);
    setError(null);
    getFreeSlots(doctor.id)
      .then(setFreeSlots)
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load slots'))
      .finally(() => setLoading(false));
  };

  useEffect(load, [doctor.id]);

  // Merge API slots with unavailable ones that aren't in the response
  const allSlots = useMemo(() => {
    const freeIds = new Set(freeSlots.map((s) => s.slotId));
    const extra = unavailableSlots.filter((s) => !freeIds.has(s.slotId));
    return [...freeSlots, ...extra].sort(
      (a, b) => new Date(a.startTimeUtc).getTime() - new Date(b.startTimeUtc).getTime(),
    );
  }, [freeSlots, unavailableSlots]);

  const isUnavailable = (id: string) => localUnavailableIds.has(id);

  const handleSlotClick = (slot: AppointmentSlotDto) => {
    if (reservingId || isUnavailable(slot.slotId)) return;
    setReserveError(null);
    setReservingId(slot.slotId);
    reserveSlot(slot.slotId)
      .then((reservation) => {
        setLocalUnavailableIds((prev) => new Set([...prev, slot.slotId]));
        onSelect(slot, reservation);
      })
      .catch((e: unknown) => {
        setLocalUnavailableIds((prev) => new Set([...prev, slot.slotId]));
        const msg = e instanceof Error ? e.message : 'Could not reserve this slot';
        setReserveError(
          msg.toLowerCase().includes('not available') || msg.toLowerCase().includes('conflict')
            ? 'This slot was just taken. Please choose another.'
            : msg,
        );
      })
      .finally(() => setReservingId(null));
  };

  if (loading) return <Spinner text="Loading available slots..." />;
  if (error) return <ErrorMessage message={error} onRetry={load} />;

  if (allSlots.length === 0) {
    return (
      <div className="empty-state">
        <div className="empty-icon">📅</div>
        <p className="empty-title">No available slots</p>
        <p className="empty-text">{doctor.fullName} has no free appointments</p>
      </div>
    );
  }

  const grouped = groupByDate(allSlots);

  return (
    <div>
      <div className="step-intro">
        <h2>Pick a time</h2>
        <p>
          {doctor.fullName} · {specialty.name}
        </p>
      </div>

      {reserveError && (
        <div className="error-message" style={{ marginBottom: 16 }}>
          <p>{reserveError}</p>
        </div>
      )}

      {grouped.map(([key, daySlots]) => (
        <div key={key} className="slot-group">
          <p className="slot-group-date">{formatDateLabel(daySlots[0].startTimeUtc)}</p>
          <div className="slot-grid">
            {daySlots.map((slot) => {
              const busy = reservingId === slot.slotId;
              const unavailable = isUnavailable(slot.slotId);
              return (
                <button
                  key={slot.slotId}
                  className={`slot-btn${busy ? ' loading' : ''}${unavailable ? ' unavailable' : ''}`}
                  disabled={!!reservingId || unavailable}
                  onClick={() => handleSlotClick(slot)}
                >
                  {busy ? (
                    <span className="slot-time">…</span>
                  ) : (
                    <>
                      <span className="slot-time">{formatTime(slot.startTimeUtc)}</span>
                      <span className="slot-end">{formatTime(slot.endTimeUtc)}</span>
                    </>
                  )}
                </button>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}
