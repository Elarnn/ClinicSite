import { useEffect, useMemo, useState } from 'react';
import type { AdminBookingDto } from '../types';
import { cancelBooking, getBookings } from '../api/bookings';

interface DoctorGroup {
  doctorId: string;
  doctorName: string;
  bookings: AdminBookingDto[];
}

interface SpecialtyGroup {
  specialtyId: string;
  specialtyName: string;
  bookingCount: number;
  doctors: DoctorGroup[];
}

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

function groupBookings(bookings: AdminBookingDto[]): SpecialtyGroup[] {
  const specialties = new Map<string, SpecialtyGroup>();

  for (const booking of bookings) {
    let specialty = specialties.get(booking.specialtyId);
    if (!specialty) {
      specialty = {
        specialtyId: booking.specialtyId,
        specialtyName: booking.specialtyName,
        bookingCount: 0,
        doctors: [],
      };
      specialties.set(booking.specialtyId, specialty);
    }

    specialty.bookingCount += 1;

    let doctor = specialty.doctors.find((d) => d.doctorId === booking.doctorId);
    if (!doctor) {
      doctor = { doctorId: booking.doctorId, doctorName: booking.doctorName, bookings: [] };
      specialty.doctors.push(doctor);
    }

    doctor.bookings.push(booking);
  }

  const groups = Array.from(specialties.values());
  groups.sort((a, b) => a.specialtyName.localeCompare(b.specialtyName));
  for (const specialty of groups) {
    specialty.doctors.sort((a, b) => a.doctorName.localeCompare(b.doctorName));
  }

  return groups;
}

export function BookingsPage() {
  const [bookings, setBookings] = useState<AdminBookingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [expandedSpecialties, setExpandedSpecialties] = useState<Set<string>>(new Set());
  const [expandedDoctors, setExpandedDoctors] = useState<Set<string>>(new Set());

  useEffect(() => {
    getBookings()
      .then(setBookings)
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'Failed to load bookings');
      })
      .finally(() => setLoading(false));
  }, []);

  const specialtyGroups = useMemo(() => groupBookings(bookings), [bookings]);

  function toggleSpecialty(specialtyId: string) {
    setExpandedSpecialties((prev) => {
      const next = new Set(prev);
      if (next.has(specialtyId)) next.delete(specialtyId);
      else next.add(specialtyId);
      return next;
    });
  }

  function toggleDoctor(doctorId: string) {
    setExpandedDoctors((prev) => {
      const next = new Set(prev);
      if (next.has(doctorId)) next.delete(doctorId);
      else next.add(doctorId);
      return next;
    });
  }

  function handleCancel(bookingId: string) {
    if (!window.confirm('Cancel this booking?')) return;

    cancelBooking(bookingId)
      .then(() => {
        setBookings((current) =>
          current.map((booking) =>
            booking.bookingId === bookingId
              ? { ...booking, isCancelled: true, status: 'Cancelled' }
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
          <p>View and manage patient appointments, grouped by specialty and doctor.</p>
        </div>
      </div>

      {bookings.length === 0 ? (
        <div className="empty-state">
          <h2>No bookings yet</h2>
          <p>Patient appointments will appear here.</p>
        </div>
      ) : (
        <div className="accordion">
          {specialtyGroups.map((specialty) => {
            const specialtyOpen = expandedSpecialties.has(specialty.specialtyId);

            return (
              <div className="accordion-item" key={specialty.specialtyId}>
                <button
                  type="button"
                  className="accordion-header"
                  onClick={() => toggleSpecialty(specialty.specialtyId)}
                  aria-expanded={specialtyOpen}
                >
                  <span className={`accordion-chevron${specialtyOpen ? ' expanded' : ''}`}>›</span>
                  <span className="accordion-title">{specialty.specialtyName}</span>
                  <span className="accordion-meta">
                    {specialty.doctors.length} doctor{specialty.doctors.length === 1 ? '' : 's'} ·{' '}
                    {specialty.bookingCount} booking{specialty.bookingCount === 1 ? '' : 's'}
                  </span>
                </button>

                {specialtyOpen && (
                  <div className="accordion-body">
                    {specialty.doctors.map((doctor) => {
                      const doctorOpen = expandedDoctors.has(doctor.doctorId);

                      return (
                        <div className="accordion-subitem" key={doctor.doctorId}>
                          <button
                            type="button"
                            className="accordion-subheader"
                            onClick={() => toggleDoctor(doctor.doctorId)}
                            aria-expanded={doctorOpen}
                          >
                            <span className={`accordion-chevron${doctorOpen ? ' expanded' : ''}`}>›</span>
                            <span className="accordion-title">{doctor.doctorName}</span>
                            <span className="accordion-meta">
                              {doctor.bookings.length} booking{doctor.bookings.length === 1 ? '' : 's'}
                            </span>
                          </button>

                          {doctorOpen && (
                            <div className="table-scroll">
                              <table className="data-table">
                                <thead>
                                  <tr>
                                    <th>Patient</th>
                                    <th>Email</th>
                                    <th>Date & Time</th>
                                    <th>Status</th>
                                    <th>Comment</th>
                                    <th>Actions</th>
                                  </tr>
                                </thead>

                                <tbody>
                                  {doctor.bookings.map((booking) => (
                                    <tr key={booking.bookingId}>
                                      <td>{booking.patientName}</td>
                                      <td>{booking.patientEmail}</td>
                                      <td>
                                        {formatDateTime(booking.startTimeUtc)} –{' '}
                                        {new Date(booking.endTimeUtc).toLocaleTimeString([], {
                                          hour: '2-digit',
                                          minute: '2-digit',
                                        })}
                                      </td>
                                      <td>{booking.status || (booking.isCancelled ? 'Cancelled' : 'Active')}</td>
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
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
