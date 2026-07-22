import { useState } from 'react';
import type { Page } from '../types';
import { BrandMark } from './BrandMark';

const NAV_LINKS: { page: Page; label: string }[] = [
  { page: 'home', label: 'Home' },
  { page: 'about', label: 'About Us' },
  { page: 'services', label: 'Services' },
  { page: 'contacts', label: 'Contacts' },
];

interface Props {
  active: Page;
  onNavigate: (page: Page) => void;
}

export function Navbar({ active, onNavigate }: Props) {
  const [menuOpen, setMenuOpen] = useState(false);

  const go = (page: Page) => {
    setMenuOpen(false);
    onNavigate(page);
  };

  return (
    <header className="site-header">
      <div className="site-header-row">
        <button className="brand" onClick={() => go('home')} aria-label="Aurelia Clinic — home">
          <BrandMark />
          <span className="brand-name">Aurelia<span className="brand-name-sub">Clinic</span></span>
        </button>

        <nav className={`site-nav${menuOpen ? ' open' : ''}`}>
          {NAV_LINKS.map((link) => (
            <button
              key={link.page}
              className={`site-nav-link${active === link.page ? ' active' : ''}`}
              onClick={() => go(link.page)}
            >
              {link.label}
            </button>
          ))}
          <button
            className={`book-btn book-btn-mobile${active === 'booking' ? ' active' : ''}`}
            onClick={() => go('booking')}
          >
            Book Appointment
          </button>
        </nav>

        <button
          className={`book-btn book-btn-desktop${active === 'booking' ? ' active' : ''}`}
          onClick={() => go('booking')}
        >
          Book Appointment
        </button>

        <button
          className={`menu-toggle${menuOpen ? ' open' : ''}`}
          aria-label="Toggle menu"
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen((v) => !v)}
        >
          <span />
          <span />
          <span />
        </button>
      </div>
    </header>
  );
}
