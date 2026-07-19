import { useCallback, useState } from 'react';
import { createBooking } from '../api/bookings';
import type {
  AppointmentSlotDto,
  BookingResultDto,
  DoctorDto,
  ReserveSlotResultDto,
  SpecialtyDto,
} from '../types';
import { Timer } from './Timer';

function formatDateTime(utc: string): string {
  const d = new Date(utc);
  return d.toLocaleDateString([], { weekday: 'short', month: 'short', day: 'numeric' }) +
    ' at ' +
    d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function formatTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

interface Props {
  specialty: SpecialtyDto;
  doctor: DoctorDto;
  slot: AppointmentSlotDto;
  reservation: ReserveSlotResultDto;
  onSuccess: (booking: BookingResultDto) => void;
  onExpired: () => void;
}

interface FormErrors {
  name?: string;
  email?: string;
}

export function BookingFormStep({ specialty, doctor, slot, reservation, onSuccess, onExpired }: Props) {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [comment, setComment] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [expired, setExpired] = useState(false);

  const handleExpired = useCallback(() => {
    setExpired(true);
  }, []);

  const validate = (): boolean => {
    const e: FormErrors = {};
    if (!name.trim() || name.trim().length < 2) e.name = 'Enter your full name (at least 2 characters)';
    if (!EMAIL_RE.test(email.trim())) e.email = 'Enter a valid email address';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = (ev: React.FormEvent) => {
    ev.preventDefault();
    if (!validate() || submitting || expired) return;
    if (!reservation.reservationToken) {
      setSubmitError('Reservation token missing. Please go back and try again.');
      return;
    }

    setSubmitting(true);
    setSubmitError(null);

    createBooking({
      appointmentSlotId: slot.slotId,
      reservationToken: reservation.reservationToken,
      patientName: name.trim(),
      patientEmail: email.trim(),
      comment: comment.trim() || undefined,
    })
      .then(onSuccess)
      .catch((e: unknown) => {
        const msg = e instanceof Error ? e.message : 'Booking failed';
        setSubmitError(
          msg.includes('409') || msg.toLowerCase().includes('conflict')
            ? 'The reservation has expired. Please go back and select a new slot.'
            : msg,
        );
      })
      .finally(() => setSubmitting(false));
  };

  return (
    <div>
      <div className="step-intro">
        <h2>Your details</h2>
        <p>Almost done — confirm your info</p>
      </div>

      {reservation.reservedUntilUtc && !expired && (
        <Timer reservedUntilUtc={reservation.reservedUntilUtc} onExpired={handleExpired} />
      )}

      {expired && (
        <div className="timer-banner expiring" style={{ marginBottom: 16 }}>
          <span className="timer-icon">⚠️</span>
          <span className="timer-text">Reservation expired.</span>
          <button
            className="btn-ghost"
            style={{ width: 'auto', marginTop: 0, padding: '6px 12px', fontSize: 13 }}
            onClick={onExpired}
          >
            Go back
          </button>
        </div>
      )}

      <div className="booking-summary">
        <div className="booking-summary-item">
          <span className="booking-summary-icon">👨‍⚕️</span>
          <div>
            <div className="booking-summary-label">Doctor</div>
            <div className="booking-summary-value">{doctor.fullName}</div>
          </div>
        </div>
        <div className="booking-summary-item">
          <span className="booking-summary-icon">🏥</span>
          <div>
            <div className="booking-summary-label">Specialty</div>
            <div className="booking-summary-value">{specialty.name}</div>
          </div>
        </div>
        <div className="booking-summary-item">
          <span className="booking-summary-icon">📅</span>
          <div>
            <div className="booking-summary-label">Date & Time</div>
            <div className="booking-summary-value">
              {formatDateTime(slot.startTimeUtc)} – {formatTime(slot.endTimeUtc)}
            </div>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} noValidate>
        <div className="form-group">
          <label className="form-label" htmlFor="patient-name">Full name</label>
          <input
            id="patient-name"
            className={`form-input${errors.name ? ' error' : ''}`}
            type="text"
            placeholder="John Smith"
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoComplete="name"
            disabled={submitting || expired}
          />
          {errors.name && <p className="form-error">{errors.name}</p>}
        </div>

        <div className="form-group">
          <label className="form-label" htmlFor="patient-email">Email</label>
          <input
            id="patient-email"
            className={`form-input${errors.email ? ' error' : ''}`}
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            inputMode="email"
            disabled={submitting || expired}
          />
          {errors.email && <p className="form-error">{errors.email}</p>}
        </div>

        <div className="form-group">
          <label className="form-label" htmlFor="patient-comment">
            Comment <span style={{ fontWeight: 400, color: '#9ca3af' }}>(optional)</span>
          </label>
          <textarea
            id="patient-comment"
            className="form-input"
            placeholder="Any notes for your doctor…"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            rows={3}
            style={{ resize: 'none' }}
            disabled={submitting || expired}
          />
        </div>

        {submitError && (
          <div className="error-message" style={{ marginBottom: 12 }}>
            <p>{submitError}</p>
          </div>
        )}

        <button
          type="submit"
          className="btn-primary"
          disabled={submitting || expired}
        >
          {submitting ? 'Booking…' : 'Confirm booking'}
        </button>
      </form>
    </div>
  );
}
