import type { Page } from '../types';
import { DoctorsTeam } from '../components/DoctorsTeam';

interface Props {
  onNavigate: (page: Page) => void;
}

const FEATURES = [
  {
    icon: '✦',
    title: 'Board-certified specialists',
    text: 'Every physician at Aurelia is fellowship-trained and hand-selected for their field.',
  },
  {
    icon: '⏱',
    title: 'Same-week appointments',
    text: 'Real-time scheduling means you see the doctor you want, on a time that suits you.',
  },
  {
    icon: '❦',
    title: 'Unhurried, private care',
    text: 'Longer consultations in quiet, comfortable suites — never rushed, always attentive.',
  },
];

export function HomePage({ onNavigate }: Props) {
  return (
    <div>
      <section className="hero">
        <div className="hero-inner">
          <p className="eyebrow">Private specialist clinic</p>
          <h1 className="hero-title">Exceptional care,<br />by appointment.</h1>
          <p className="hero-subtitle">
            Aurelia Clinic brings together leading specialists across cardiology, dermatology,
            pediatrics and more — in a setting built around your time and comfort.
          </p>
          <div className="hero-actions">
            <button className="btn-gold" onClick={() => onNavigate('booking')}>
              Book Appointment
            </button>
            <button className="btn-outline-light" onClick={() => onNavigate('services')}>
              View Services
            </button>
          </div>
        </div>
      </section>

      <section className="section">
        <div className="section-inner">
          <p className="eyebrow eyebrow-dark">Why Aurelia</p>
          <h2 className="section-title">A different standard of care</h2>

          <div className="feature-grid">
            {FEATURES.map((f) => (
              <div className="feature-card" key={f.title}>
                <span className="feature-icon">{f.icon}</span>
                <h3>{f.title}</h3>
                <p>{f.text}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <DoctorsTeam />

      <section className="cta-banner">
        <div className="cta-banner-inner">
          <h2>Ready to be seen?</h2>
          <p>Choose your specialist and pick a time that works — in under two minutes.</p>
          <button className="btn-gold" onClick={() => onNavigate('booking')}>
            Book Appointment
          </button>
        </div>
      </section>
    </div>
  );
}
