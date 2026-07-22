import { useState } from 'react';
import './App.css';
import type { Page } from './types';
import { Navbar } from './components/Navbar';
import { Footer } from './components/Footer';
import { HomePage } from './pages/HomePage';
import { AboutPage } from './pages/AboutPage';
import { ServicesPage } from './pages/ServicesPage';
import { ContactsPage } from './pages/ContactsPage';
import { BookingPage } from './pages/BookingPage';

export default function App() {
  const [page, setPage] = useState<Page>('home');

  const navigate = (next: Page) => {
    setPage(next);
    window.scrollTo({ top: 0, behavior: 'instant' as ScrollBehavior });
  };

  return (
    <div className="site">
      <Navbar active={page} onNavigate={navigate} />

      <main className="site-main">
        {page === 'home' && <HomePage onNavigate={navigate} />}
        {page === 'about' && <AboutPage onNavigate={navigate} />}
        {page === 'services' && <ServicesPage onNavigate={navigate} />}
        {page === 'contacts' && <ContactsPage onNavigate={navigate} />}
        {page === 'booking' && <BookingPage />}
      </main>

      {page !== 'booking' && <Footer onNavigate={navigate} />}
    </div>
  );
}
