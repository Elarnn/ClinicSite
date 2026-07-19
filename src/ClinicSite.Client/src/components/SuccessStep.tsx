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
      <div className="success-icon">✅</div>
      <h2 className="success-title">Booking confirmed!</h2>
      <p className="success-subtitle">
        We'll see you soon, {booking.patientName.split(' ')[0]}.
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
            <div className="success-detail-label">Date & Time</div>
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
        <div className="success-detail">
          <span className="success-detail-icon">✉️</span>
          <div className="success-detail-body">
            <div className="success-detail-label">Email</div>
            <div className="success-detail-value">{booking.patientEmail}</div>
          </div>
        </div>
        {booking.comment && (
          <div className="success-detail">
            <span className="success-detail-icon">💬</span>
            <div className="success-detail-body">
              <div className="success-detail-label">Note</div>
              <div className="success-detail-value">{booking.comment}</div>
            </div>
          </div>
        )}
        <div className="success-detail">
          <span className="success-detail-icon">#</span>
          <div className="success-detail-body">
            <div className="success-detail-label">Booking ID</div>
            <div className="success-detail-value" style={{ fontSize: 12, fontFamily: 'monospace' }}>
              {booking.bookingId}
            </div>
          </div>
        </div>
      </div>

      <button className="btn-ghost" onClick={onBookAgain} style={{ width: '100%' }}>
        Book another appointment
      </button>
    </div>
  );
}
