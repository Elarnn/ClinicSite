import { useEffect, useState } from 'react';
import { getMyBookings } from '../api/bookings';
import { ApiError } from '../api/http';
import type { DoctorBooking } from '../types';
import type { DoctorSession } from '../auth/session';

interface Props {
  session: DoctorSession;
  onLogout: () => void;
}

function formatDate(utc: string): string {
  return new Date(utc).toLocaleDateString([], {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

function formatTime(utc: string): string {
  return new Date(utc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function statusClass(status: string): string {
  return `badge badge-${status.toLowerCase()}`;
}

export function BookingsPage({ session, onLogout }: Props) {
  const [bookings, setBookings] = useState<DoctorBooking[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getMyBookings()
      .then(setBookings)
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to load your bookings.'));
  }, []);

  return (
    <div className="portal">
      <header className="portal-header">
        <div className="brand">ClinicSite <span>Doctor Portal</span></div>
        <div className="portal-header-right">
          <span className="portal-user">{session.doctorName}</span>
          <button className="btn-ghost" onClick={onLogout}>Sign out</button>
        </div>
      </header>

      <main className="portal-main">
        <h1 className="page-title">My bookings</h1>
        <p className="page-subtitle">Appointments booked for your slots, most recent first.</p>

        {error && <p className="field-error">{error}</p>}

        {!error && bookings === null && <p className="muted">Loading…</p>}

        {bookings !== null && bookings.length === 0 && (
          <div className="empty">
            <div className="empty-icon">📅</div>
            <p>No bookings yet.</p>
          </div>
        )}

        {bookings !== null && bookings.length > 0 && (
          <div className="table-scroll">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Time</th>
                  <th>Patient</th>
                  <th>Email</th>
                  <th>Comment</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((b, i) => (
                  <tr key={i}>
                    <td>{formatDate(b.startTimeUtc)}</td>
                    <td>{formatTime(b.startTimeUtc)} – {formatTime(b.endTimeUtc)}</td>
                    <td>{b.patientName}</td>
                    <td>{b.patientEmail}</td>
                    <td className="comment-cell">{b.comment ?? '—'}</td>
                    <td><span className={statusClass(b.status)}>{b.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </div>
  );
}
