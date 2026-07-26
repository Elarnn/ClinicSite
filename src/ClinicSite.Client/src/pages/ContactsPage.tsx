import type { Page } from '../types';

interface Props {
  onNavigate: (page: Page) => void;
}

const ADDRESS = '250 Park Avenue, Suite 400, New York, NY 10177';

// Embedding via maps.google.com/maps?...&output=embed needs no API key, unlike the official
// Maps Embed API (google.com/maps/embed/v1/place), which requires a billing-enabled key.
const MAP_EMBED_URL = `https://maps.google.com/maps?q=${encodeURIComponent(ADDRESS)}&z=15&output=embed`;
const MAP_LINK_URL = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(ADDRESS)}`;

const DETAILS = [
  { icon: '⌂', label: 'Address', value: ADDRESS, href: MAP_LINK_URL },
  { icon: '☏', label: 'Phone', value: '+1 (555) 010-2200', href: 'tel:+15550102200' },
  { icon: '✉', label: 'Email', value: 'care@aurelia-clinic.example', href: 'mailto:care@aurelia-clinic.example' },
  { icon: '⏱', label: 'Hours', value: 'Mon–Sat, 8:00–20:00', href: null },
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
                  <div className="contact-value">
                    {d.href ? (
                      <a
                        className="contact-link"
                        href={d.href}
                        target={d.href.startsWith('http') ? '_blank' : undefined}
                        rel={d.href.startsWith('http') ? 'noreferrer' : undefined}
                      >
                        {d.value}
                      </a>
                    ) : (
                      d.value
                    )}
                  </div>
                </div>
              </div>
            ))}
            <button className="btn-gold" style={{ marginTop: 12 }} onClick={() => onNavigate('booking')}>
              Book Appointment
            </button>
          </div>

          <div className="map-embed">
            <iframe
              title={`Map showing Aurelia Clinic at ${ADDRESS}`}
              src={MAP_EMBED_URL}
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
              allowFullScreen
            />
          </div>
        </div>
      </section>
    </div>
  );
}
