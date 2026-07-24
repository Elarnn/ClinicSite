import { useEffect, useRef, useState } from 'react';
import { confirmBooking } from '../api/bookings';
import { ApiError } from '../api/client';
import type { ConfirmBookingResultDto } from '../types';
import { BrandMark } from '../components/BrandMark';

type Status =
  | { kind: 'loading' }
  | { kind: 'success'; booking: ConfirmBookingResultDto }
  | { kind: 'expired' }
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

export function ConfirmBookingPage({ onHome }: Props) {
  const [status, setStatus] = useState<Status>({ kind: 'loading' });
  // Guard against React StrictMode invoking the effect twice in development.
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const token = new URLSearchParams(window.location.search).get('token');

    // Remove the token from the address bar as early as possible (never stored, never logged).
    window.history.replaceState({}, '', '/booking/confirm');

    if (!token) {
      setStatus({ kind: 'invalid' });
      return;
    }

    confirmBooking(token)
      .then((booking) => setStatus({ kind: 'success', booking }))
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 410) {
          setStatus({ kind: 'expired' });
        } else {
          setStatus({ kind: 'invalid' });
        }
      });
  }, []);

  return (
    <div className="status-shell">
      <div className="status-card">
        <BrandMark />

        {status.kind === 'loading' && (
          <div className="status-body">
            <div className="status-spinner" aria-hidden />
            <h1 className="status-title">Confirming your appointment…</h1>
            <p className="status-subtitle">Please wait a couple of seconds.</p>
          </div>
        )}

        {status.kind === 'success' && (
          <div className="status-body">
            <div className="status-icon success">✅</div>
            <h1 className="status-title">Your appointment is confirmed</h1>
            <p className="status-subtitle">
              Thank you, {status.booking.patientName.split(' ')[0]}! We look forward to seeing you.
            </p>

            <div className="status-details">
              <div className="status-detail">
                <span className="status-detail-label">Doctor</span>
                <span className="status-detail-value">{status.booking.doctorName}</span>
              </div>
              <div className="status-detail">
                <span className="status-detail-label">Specialty</span>
                <span className="status-detail-value">{status.booking.specialtyName}</span>
              </div>
              <div className="status-detail">
                <span className="status-detail-label">Date</span>
                <span className="status-detail-value">{formatDate(status.booking.startTimeUtc)}</span>
              </div>
              <div className="status-detail">
                <span className="status-detail-label">Time</span>
                <span className="status-detail-value">
                  {formatTime(status.booking.startTimeUtc)} – {formatTime(status.booking.endTimeUtc)}
                </span>
              </div>
            </div>

            <button className="btn-primary" onClick={onHome}>
              Back to home
            </button>
          </div>
        )}

        {status.kind === 'expired' && (
          <div className="status-body">
            <div className="status-icon warning">⏰</div>
            <h1 className="status-title">The confirmation link has expired</h1>
            <p className="status-subtitle">
              The slot has been released and is available to book again. Please make a new booking.
            </p>
            <button className="btn-primary" onClick={onHome}>
              Book again
            </button>
          </div>
        )}

        {status.kind === 'invalid' && (
          <div className="status-body">
            <div className="status-icon error">⚠️</div>
            <h1 className="status-title">This link is invalid or has expired</h1>
            <p className="status-subtitle">
              Check the link from the e-mail or make a new booking.
            </p>
            <button className="btn-primary" onClick={onHome}>
              Back to home
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
