import type { Page } from '../types';
import { BrandMark } from './BrandMark';

interface Props {
  onNavigate: (page: Page) => void;
}

export function Footer({ onNavigate }: Props) {
  return (
    <footer className="site-footer">
      <div className="footer-inner">
        <div className="footer-brand">
          <BrandMark />
          <span className="brand-name">Aurelia<span className="brand-name-sub">Clinic</span></span>
          <p className="footer-tagline">Private specialist care, by appointment.</p>
        </div>

        <div className="footer-col">
          <h4>Navigate</h4>
          <button onClick={() => onNavigate('home')}>Home</button>
          <button onClick={() => onNavigate('about')}>About Us</button>
          <button onClick={() => onNavigate('services')}>Services</button>
          <button onClick={() => onNavigate('contacts')}>Contacts</button>
        </div>

        <div className="footer-col">
          <h4>Visit</h4>
          <p>12 Magnolia Avenue<br />Suite 400</p>
          <p>Mon–Sat · 8:00–20:00</p>
        </div>

        <div className="footer-col">
          <h4>Contact</h4>
          <p>+1 (555) 010-2200</p>
          <p>care@aurelia-clinic.example</p>
        </div>
      </div>

      <div className="footer-bottom">
        <span>© {new Date().getFullYear()} Aurelia Clinic. All rights reserved.</span>
      </div>
    </footer>
  );
}
