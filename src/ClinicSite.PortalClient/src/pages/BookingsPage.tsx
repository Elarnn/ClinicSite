import { useEffect, useState } from 'react';
import type { AdminBookingDto } from '../types';
import { cancelBooking, getBookings } from '../api/bookings';

function formatDateTime(value: string) {
  const date = new Date(value);

  return date.toLocaleDateString([], {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }) + ' ' + date.toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function BookingsPage() {
  const [bookings, setBookings] = useState<AdminBookingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getBookings()
      .then(setBookings)
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'Failed to load bookings');
      })
      .finally(() => setLoading(false));
  }, []);

  function handleCancel(bookingId: string) {
    if (!window.confirm('Cancel this booking?')) return;

    cancelBooking(bookingId)
      .then(() => {
        setBookings((current) =>
          current.map((booking) =>
            booking.bookingId === bookingId
              ? { ...booking, isCancelled: true }
              : booking
          )
        );
      })
      .catch((e: unknown) => {
        alert(e instanceof Error ? e.message : 'Failed to cancel booking');
      });
  }

  if (loading) return <p>Loading bookings...</p>;
  if (error) return <p className="error-text">{error}</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Bookings</h1>
          <p>View and manage patient appointments.</p>
        </div>
      </div>

      {bookings.length === 0 ? (
        <div className="empty-state">
          <h2>No bookings yet</h2>
          <p>Patient appointments will appear here.</p>
        </div>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Patient</th>
              <th>Email</th>
              <th>Doctor</th>
              <th>Date & Time</th>
              <th>Status</th>
              <th>Comment</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            {bookings.map((booking) => (
              <tr key={booking.bookingId}>
                <td>{booking.patientName}</td>
                <td>{booking.patientEmail}</td>
                <td>{booking.doctorName}</td>
                <td>
                  {formatDateTime(booking.startTimeUtc)} –{' '}
                  {new Date(booking.endTimeUtc).toLocaleTimeString([], {
                    hour: '2-digit',
                    minute: '2-digit',
                  })}
                </td>
                <td>
                  {booking.isCancelled ? 'Cancelled' : 'Active'}
                </td>
                <td>{booking.comment || '—'}</td>
                <td>
                  {booking.isCancelled ? (
                    <span>—</span>
                  ) : (
                    <button
                      className="small-button danger"
                      onClick={() => handleCancel(booking.bookingId)}
                    >
                      Cancel
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}