import type { Dashboard } from '../types';
import { fmtTime } from '../lib/format';

function Card({ label, value }: { label: string; value: number | string }) {
  return (
    <div className="stat-card">
      <span className="stat-label">{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

export function DoctorSummaryCards({ dashboard }: { dashboard: Dashboard }) {
  const next = dashboard.nextPatientStartUtc ? fmtTime(dashboard.nextPatientStartUtc) : '—';
  return (
    <div className="cards-grid">
      <Card label="Today's bookings" value={dashboard.todayCount} />
      <Card label="Remaining" value={dashboard.remainingCount} />
      <Card label="Next patient" value={next} />
      <Card label="Free windows" value={dashboard.freeWindowsCount} />
    </div>
  );
}
