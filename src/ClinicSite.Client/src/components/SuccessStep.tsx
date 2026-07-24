import type { BookingResultDto } from '../types';

function formatDateTime(utc: string): string {
  const d = new Date(utc);
  return (
    d.toLocaleDateString([], { weekday: 'long', month: 'long', day: 'numeric' }) +
    ' · ' +
    d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  );
}

function formatTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

interface Props {
  booking: BookingResultDto;
  onBookAgain: () => void;
}

export function SuccessStep({ booking, onBookAgain }: Props) {
  return (
    <div className="success-screen">
      <div className="success-icon">✉️</div>
      <h2 className="success-title">Check your email</h2>
      <p className="success-subtitle">
        We've sent an email to <strong>{booking.patientEmail}</strong>. Please confirm your booking
        within 30 minutes, otherwise the slot will be released.
      </p>

      <div className="success-card">
        <div className="success-detail">
          <span className="success-detail-icon">👨‍⚕️</span>
          <div className="success-detail-body">
            <div className="success-detail-label">Doctor</div>
            <div className="success-detail-value">{booking.doctorName}</div>
          </div>
        </div>
        <div className="success-detail">
          <span className="success-detail-icon">📅</span>
          <div className="success-detail-body">
            <div className="success-detail-label">Date & time</div>
            <div className="success-detail-value">
              {formatDateTime(booking.startTimeUtc)} – {formatTime(booking.endTimeUtc)}
            </div>
          </div>
        </div>
        <div className="success-detail">
          <span className="success-detail-icon">👤</span>
          <div className="success-detail-body">
            <div className="success-detail-label">Patient</div>
            <div className="success-detail-value">{booking.patientName}</div>
          </div>
        </div>
        {booking.comment && (
          <div className="success-detail">
            <span className="success-detail-icon">💬</span>
            <div className="success-detail-body">
              <div className="success-detail-label">Comment</div>
              <div className="success-detail-value">{booking.comment}</div>
            </div>
          </div>
        )}
      </div>

      <p className="success-hint">
        Didn't get the email? Check your spam folder or book again.
      </p>

      <button className="btn-ghost" onClick={onBookAgain} style={{ width: '100%' }}>
        Book another appointment
      </button>
    </div>
  );
}
