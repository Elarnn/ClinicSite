import { useEffect, useState } from 'react';
import { getDoctors } from '../api/doctors';
import { API_BASE_URL } from '../api/client';
import type { DoctorDto } from '../types';

const INITIAL_COUNT = 3;

function photoUrl(id: string): string {
  return `${API_BASE_URL}/api/doctors/${id}/photo`;
}

function initials(name: string): string {
  return name
    .split(' ')
    .filter(Boolean)
    .map((w) => w[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

function TeamCard({ doctor }: { doctor: DoctorDto }) {
  const [imgFailed, setImgFailed] = useState(false);
  const showPhoto = doctor.hasPhoto && !imgFailed;

  return (
    <div className="team-card">
      <div className="team-photo">
        {showPhoto ? (
          <img
            src={photoUrl(doctor.id)}
            alt={doctor.fullName}
            loading="lazy"
            onError={() => setImgFailed(true)}
          />
        ) : (
          <span className="team-photo-fallback">{initials(doctor.fullName)}</span>
        )}
      </div>
      <h3 className="team-name">{doctor.fullName}</h3>
      <p className="team-specialty">{doctor.specialtyName}</p>
    </div>
  );
}

export function DoctorsTeam() {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [failed, setFailed] = useState(false);
  const [expanded, setExpanded] = useState(false);

  useEffect(() => {
    getDoctors()
      .then(setDoctors)
      .catch(() => setFailed(true))
      .finally(() => setLoading(false));
  }, []);

  // Nothing to show yet, or the clinic has no doctors — keep the home page clean.
  if (loading || failed || doctors.length === 0) {
    return null;
  }

  const shown = expanded ? doctors : doctors.slice(0, INITIAL_COUNT);
  const hasMore = doctors.length > INITIAL_COUNT;

  return (
    <section className="section team-section">
      <div className="section-inner">
        <p className="eyebrow eyebrow-dark">Our team</p>
        <h2 className="section-title">Meet the doctors</h2>

        <div className="team-grid">
          {shown.map((d) => (
            <TeamCard key={d.id} doctor={d} />
          ))}
        </div>

        {hasMore && (
          <div className="team-more">
            <button className="btn-outline-dark" onClick={() => setExpanded((v) => !v)}>
              {expanded ? 'Show fewer' : `Show all ${doctors.length} doctors`}
            </button>
          </div>
        )}
      </div>
    </section>
  );
}
