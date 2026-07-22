import type { Page } from '../types';

interface Props {
  onNavigate: (page: Page) => void;
}

const DETAILS = [
  { icon: '⌂', label: 'Address', value: '12 Magnolia Avenue, Suite 400' },
  { icon: '☏', label: 'Phone', value: '+1 (555) 010-2200' },
  { icon: '✉', label: 'Email', value: 'care@aurelia-clinic.example' },
  { icon: '⏱', label: 'Hours', value: 'Mon–Sat, 8:00–20:00' },
];

export function ContactsPage({ onNavigate }: Props) {
  return (
    <div>
      <section className="page-hero">
        <div className="page-hero-inner">
          <p className="eyebrow">Contacts</p>
          <h1>We'd love to see you</h1>
          <p className="page-hero-subtitle">
            Reach our care team directly, or book online in a couple of minutes.
          </p>
        </div>
      </section>

      <section className="section">
        <div className="section-inner contacts-layout">
          <div className="contact-list">
            {DETAILS.map((d) => (
              <div className="contact-item" key={d.label}>
                <span className="contact-icon">{d.icon}</span>
                <div>
                  <div className="contact-label">{d.label}</div>
                  <div className="contact-value">{d.value}</div>
                </div>
              </div>
            ))}
            <button className="btn-gold" style={{ marginTop: 12 }} onClick={() => onNavigate('booking')}>
              Book Appointment
            </button>
          </div>

          <div className="map-placeholder" aria-hidden="true">
            <span>Aurelia Clinic · 12 Magnolia Avenue</span>
          </div>
        </div>
      </section>
    </div>
  );
}
