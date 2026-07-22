import type { Page } from '../types';

interface Props {
  onNavigate: (page: Page) => void;
}

const SERVICES = [
  {
    icon: '♥',
    title: 'Cardiology',
    text: 'Heart health screening, diagnostics and ongoing management.',
    photo: '/images/services/cardiology.jpg',
  },
  {
    icon: '❁',
    title: 'Dermatology',
    text: 'Medical and cosmetic skin care from certified dermatologists.',
    photo: '/images/services/dermatology.jpg',
  },
  {
    icon: '☺',
    title: 'Pediatrics',
    text: 'Attentive, family-centred care for infants through adolescents.',
    photo: '/images/services/pediatrics.jpg',
  },
  {
    icon: '✳',
    title: 'Neurology',
    text: 'Evaluation and treatment for the brain and nervous system.',
    photo: '/images/services/neurology.jpg',
  },
  {
    icon: '✚',
    title: 'Orthopedics',
    text: 'Joint, bone and sports-injury care from consultation to recovery.',
    photo: '/images/services/orthopedics.jpg',
  },
  {
    icon: '☾',
    title: 'Dentistry',
    text: 'General and cosmetic dentistry in a calm, private setting.',
    photo: '/images/services/dentistry.jpg',
  },
];

export function ServicesPage({ onNavigate }: Props) {
  return (
    <div>
      <section className="page-hero">
        <div className="page-hero-inner">
          <p className="eyebrow">Services</p>
          <h1>Specialties under one roof</h1>
          <p className="page-hero-subtitle">
            A curated set of departments, each led by specialists who trained for it — so you
            never have to compromise on who treats you.
          </p>
        </div>
      </section>

      <section className="section">
        <div className="section-inner">
          <div className="service-grid">
            {SERVICES.map((s) => (
              <div className="service-card" key={s.title}>
                <img
                  className="service-photo"
                  src={s.photo}
                  alt=""
                  onError={(e) => {
                    e.currentTarget.style.display = 'none';
                  }}
                />
                <span className="service-icon">{s.icon}</span>
                <h3>{s.title}</h3>
                <p>{s.text}</p>
                <button className="service-link" onClick={() => onNavigate('booking')}>
                  Book a consultation →
                </button>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="cta-banner">
        <div className="cta-banner-inner">
          <h2>Don't see your specialty?</h2>
          <p>Reach out and our care team will match you with the right physician.</p>
          <button className="btn-gold" onClick={() => onNavigate('contacts')}>
            Contact Us
          </button>
        </div>
      </section>
    </div>
  );
}
