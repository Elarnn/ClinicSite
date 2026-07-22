import { useEffect, useState } from 'react';
import { getDoctors } from '../api/doctors';
import type { DoctorDto, SpecialtyDto } from '../types';
import { Spinner } from './Spinner';
import { ErrorMessage } from './ErrorMessage';

function initials(fullName: string): string {
  return fullName
    .split(' ')
    .filter(Boolean)
    .map((w) => w[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

function avatarUrl(doctor: DoctorDto): string {
  return `https://i.pravatar.cc/96?u=${doctor.id}`;
}

interface Props {
  specialty: SpecialtyDto;
  onSelect: (doctor: DoctorDto) => void;
}

export function DoctorStep({ specialty, onSelect }: Props) {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = () => {
    setLoading(true);
    setError(null);
    getDoctors(specialty.id)
      .then(setDoctors)
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load doctors'))
      .finally(() => setLoading(false));
  };

  useEffect(load, [specialty.id]);

  if (loading) return <Spinner text="Loading doctors..." />;
  if (error) return <ErrorMessage message={error} onRetry={load} />;

  if (doctors.length === 0) {
    return (
      <div className="empty-state">
        <div className="empty-icon">👨‍⚕️</div>
        <p className="empty-title">No doctors available</p>
        <p className="empty-text">No doctors found for {specialty.name}</p>
      </div>
    );
  }

  return (
    <div>
      <div className="step-intro">
        <h2>Choose a doctor</h2>
        <p>{specialty.name} specialists</p>
      </div>
      <div className="card-list">
        {doctors.map((d) => (
          <button key={d.id} className="card" onClick={() => onSelect(d)}>
            <span className="card-icon doctor-avatar">
              <img
                src={avatarUrl(d)}
                alt=""
                onError={(e) => {
                  e.currentTarget.style.display = 'none';
                  e.currentTarget.nextElementSibling?.classList.remove('hidden');
                }}
              />
              <span className="doctor-avatar-fallback hidden">{initials(d.fullName)}</span>
            </span>
            <span className="card-body">
              <span className="card-title">{d.fullName}</span>
              <span className="card-subtitle">{d.specialtyName}</span>
            </span>
            <span className="card-arrow">›</span>
          </button>
        ))}
      </div>
    </div>
  );
}
