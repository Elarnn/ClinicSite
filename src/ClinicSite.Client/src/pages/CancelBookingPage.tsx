import { useEffect, useRef, useState } from 'react';
import { cancelBooking, getCancelInfo } from '../api/bookings';
import { ApiError } from '../api/client';
import type { BookingSummaryDto, CancelBookingResultDto } from '../types';
import { BrandMark } from '../components/BrandMark';

type Status =
  | { kind: 'loading' }
  | { kind: 'confirm'; info: BookingSummaryDto }
  | { kind: 'not-cancellable'; info: BookingSummaryDto }
  | { kind: 'cancelling'; info: BookingSummaryDto }
  | { kind: 'done'; result: CancelBookingResultDto }
  | { kind: 'invalid' };

function formatDate(utc: string): string {
  return new Date(utc).toLocaleDateString([], {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  });
}

function formatTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

interface Props {
  onHome: () => void;
}

export function CancelBookingPage({ onHome }: Props) {
  const [status, setStatus] = useState<Status>({ kind: 'loading' });
  const [error, setError] = useState<string | null>(null);
  // Token is held in memory only — never in localStorage / sessionStorage.
  const tokenRef = useRef<string | null>(null);
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const token = new URLSearchParams(window.location.search).get('token');
    tokenRef.current = token;

    // Strip the token from the address bar; keep it only in the ref above.
    window.history.replaceState({}, '', '/booking/cancel');

    if (!token) {
      setStatus({ kind: 'invalid' });
      return;
    }

    getCancelInfo(token)
      .then((info) => {
        setStatus(info.cancellable ? { kind: 'confirm', info } : { kind: 'not-cancellable', info });
      })
      .catch(() => setStatus({ kind: 'invalid' }));
  }, []);

  function handleCancel(info: BookingSummaryDto) {
    const token = tokenRef.current;
    if (!token) {
      setStatus({ kind: 'invalid' });
      return;
    }

    setError(null);
    setStatus({ kind: 'cancelling', info });

    cancelBooking(token)
      .then((result) => setStatus({ kind: 'done', result }))
      .catch((e: unknown) => {
        const message =
          e instanceof ApiError ? e.message : 'Could not cancel the booking. Please try again later.';
        setError(message);
        setStatus({ kind: 'confirm', info });
      });
  }

  const details = (info: BookingSummaryDto) => (
    <div className="status-details">
      <div className="status-detail">
        <span className="status-detail-label">Doctor</span>
        <span className="status-detail-value">{info.doctorName}</span>
      </div>
      <div className="status-detail">
        <span className="status-detail-label">Specialty</span>
        <span className="status-detail-value">{info.specialtyName}</span>
      </div>
      <div className="status-detail">
        <span className="status-detail-label">Date</span>
        <span className="status-detail-value">{formatDate(info.startTimeUtc)}</span>
      </div>
      <div className="status-detail">
        <span className="status-detail-label">Time</span>
        <span className="status-detail-value">
          {formatTime(info.startTimeUtc)} – {formatTime(info.endTimeUtc)}
        </span>
      </div>
    </div>
  );

  return (
    <div className="status-shell">
      <div className="status-card">
        <BrandMark />

        {status.kind === 'loading' && (
          <div className="status-body">
            <div className="status-spinner" aria-hidden />
            <h1 className="status-title">Loading your booking…</h1>
          </div>
        )}

        {(status.kind === 'confirm' || status.kind === 'cancelling') && (
          <div className="status-body">
            <div className="status-icon warning">🗓️</div>
            <h1 className="status-title">Cancel this appointment?</h1>
            <p className="status-subtitle">Review the appointment details before cancelling.</p>

            {details(status.info)}

            {error && (
              <div className="error-message" style={{ marginBottom: 12 }}>
                <p>{error}</p>
              </div>
            )}

            <button
              className="btn-danger"
              onClick={() => handleCancel(status.info)}
              disabled={status.kind === 'cancelling'}
            >
              {status.kind === 'cancelling' ? 'Cancelling…' : 'Yes, cancel appointment'}
            </button>
            <button className="btn-ghost" onClick={onHome} disabled={status.kind === 'cancelling'}>
              Keep appointment
            </button>
          </div>
        )}

        {status.kind === 'not-cancellable' && (
          <div className="status-body">
            <div className="status-icon warning">ℹ️</div>
            <h1 className="status-title">
              {status.info.status === 'Cancelled' ? 'This appointment is already cancelled' : 'This appointment cannot be cancelled'}
            </h1>
            <p className="status-subtitle">
              {status.info.status === 'Cancelled'
                ? 'This appointment was cancelled earlier.'
                : 'Cancellation is only available for a confirmed appointment before it starts.'}
            </p>
            <button className="btn-primary" onClick={onHome}>
              Back to home
            </button>
          </div>
        )}

        {status.kind === 'done' && (
          <div className="status-body">
            <div className="status-icon success">✅</div>
            <h1 className="status-title">Your appointment has been cancelled</h1>
            <p className="status-subtitle">
              The slot is free again. You can make a new booking at any time.
            </p>
            <button className="btn-primary" onClick={onHome}>
              Back to home
            </button>
          </div>
        )}

        {status.kind === 'invalid' && (
          <div className="status-body">
            <div className="status-icon error">⚠️</div>
            <h1 className="status-title">This link is invalid or has expired</h1>
            <p className="status-subtitle">Check the link from the e-mail.</p>
            <button className="btn-primary" onClick={onHome}>
              Back to home
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
