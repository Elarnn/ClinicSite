import { useState } from 'react';
import './App.css';

import { DashboardPage } from './pages/DashboardPage';
import { DoctorsPage } from './pages/DoctorsPage';
import { SpecialtiesPage } from './pages/SpecialtiesPage';
import { BookingsPage } from './pages/BookingsPage';
import { SchedulePage } from './pages/SchedulePage';

type Page = 'Dashboard' | 'Doctors' | 'Specialties' | 'Bookings' | 'Schedule';

const pages: Page[] = ['Dashboard', 'Doctors', 'Specialties', 'Bookings', 'Schedule'];

export default function App() {
  const [activePage, setActivePage] = useState<Page>('Dashboard');

  function renderPage() {
    if (activePage === 'Dashboard') return <DashboardPage />;
    if (activePage === 'Doctors') return <DoctorsPage />;
    if (activePage === 'Specialties') return <SpecialtiesPage />;
    if (activePage === 'Bookings') return <BookingsPage />;
    if (activePage === 'Schedule') return <SchedulePage />;

    return <DashboardPage />;
  }

  return (
    <div className="portal">
      <aside className="sidebar">
        <div className="sidebar-logo">Clinique Portal</div>

        <nav className="sidebar-nav">
          {pages.map((page) => (
            <button
              key={page}
              className={activePage === page ? 'nav-button active' : 'nav-button'}
              onClick={() => setActivePage(page)}
            >
              {page}
            </button>
          ))}
        </nav>
      </aside>

      <main className="portal-main">
        {renderPage()}
      </main>
    </div>
  );
}