import type { Page } from '../types';

interface Props {
  onNavigate: (page: Page) => void;
}

const STATS = [
  { value: '18+', label: 'Years in practice' },
  { value: '32', label: 'Resident specialists' },
  { value: '40,000+', label: 'Patients cared for' },
];

const VALUES = [
  { title: 'Precision', text: 'Diagnoses grounded in evidence, second-opinion rigor and modern imaging.' },
  { title: 'Discretion', text: 'Private suites and a care team that respects your time and privacy.' },
  { title: 'Continuity', text: 'One care record, one point of contact, across every specialty you visit.' },
];

export function AboutPage({ onNavigate }: Props) {
  return (
    <div>
      <section className="page-hero">
        <div className="page-hero-inner">
          <p className="eyebrow">About Us</p>
          <h1>A clinic built around the patient</h1>
          <p className="page-hero-subtitle">
            Aurelia Clinic was founded on a simple idea: exceptional medicine and genuine
            hospitality are not in tension — they belong together.
          </p>
        </div>
      </section>

      <section className="section">
        <div className="section-inner">
          <div className="stat-row">
            {STATS.map((s) => (
              <div className="stat-block" key={s.label}>
                <div className="stat-value">{s.value}</div>
                <div className="stat-caption">{s.label}</div>
              </div>
            ))}
          </div>

          <p className="eyebrow eyebrow-dark" style={{ marginTop: 56 }}>Our Values</p>
          <h2 className="section-title">What guides our care</h2>

          <div className="feature-grid">
            {VALUES.map((v) => (
              <div className="feature-card" key={v.title}>
                <h3>{v.title}</h3>
                <p>{v.text}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="cta-banner">
        <div className="cta-banner-inner">
          <h2>Meet your specialist</h2>
          <p>Browse our departments and reserve a consultation online.</p>
          <button className="btn-gold" onClick={() => onNavigate('booking')}>
            Book Appointment
          </button>
        </div>
      </section>
    </div>
  );
}
