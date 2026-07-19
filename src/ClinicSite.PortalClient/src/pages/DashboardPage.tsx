export function DashboardPage() {
  return (
    <div>
      <h1>Dashboard</h1>
      <p>Internal clinic workspace.</p>

      <div className="cards-grid">
        <div className="stat-card">
          <span className="stat-label">Today bookings</span>
          <strong>—</strong>
        </div>

        <div className="stat-card">
          <span className="stat-label">Doctors</span>
          <strong>—</strong>
        </div>

        <div className="stat-card">
          <span className="stat-label">Specialties</span>
          <strong>—</strong>
        </div>
      </div>
    </div>
  );
}