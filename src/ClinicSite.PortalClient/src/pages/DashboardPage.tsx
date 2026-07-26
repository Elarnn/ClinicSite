import { useEffect, useState } from 'react';
import { getBookings } from '../api/bookings';
import { getAdminDoctors } from '../api/doctors';
import { getSpecialties } from '../api/specialties';

interface Stats {
  todayBookings: number;
  doctors: number;
  specialties: number;
}

function isToday(utc: string): boolean {
  const d = new Date(utc);
  const now = new Date();
  return (
    d.getFullYear() === now.getFullYear() &&
    d.getMonth() === now.getMonth() &&
    d.getDate() === now.getDate()
  );
}

export function DashboardPage() {
  const [stats, setStats] = useState<Stats | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([getBookings(), getAdminDoctors(), getSpecialties()])
      .then(([bookings, doctors, specialties]) => {
        // "Today's bookings": appointments scheduled for today that are still active.
        const isActive = (status: string) => status !== 'Cancelled' && status !== 'Expired';
        const todayBookings = bookings.filter((b) => isActive(b.status) && isToday(b.startTimeUtc)).length;

        setStats({
          todayBookings,
          doctors: doctors.length,
          specialties: specialties.length,
        });
      })
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load statistics'));
  }, []);

  // "—" on error, "…" while loading, the number once loaded.
  const show = (value: number | undefined): string =>
    error ? '—' : value === undefined ? '…' : String(value);

  return (
    <div>
      <h1>Dashboard</h1>
      <p>Internal clinic workspace.</p>

      {error && <p className="error-text">{error}</p>}

      <div className="cards-grid">
        <div className="stat-card">
          <span className="stat-label">Today bookings</span>
          <strong>{show(stats?.todayBookings)}</strong>
        </div>

        <div className="stat-card">
          <span className="stat-label">Doctors</span>
          <strong>{show(stats?.doctors)}</strong>
        </div>

        <div className="stat-card">
          <span className="stat-label">Specialties</span>
          <strong>{show(stats?.specialties)}</strong>
        </div>
      </div>
    </div>
  );
}
