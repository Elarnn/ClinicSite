import { useEffect, useState } from 'react';
import { getSpecialties } from '../api/specialties';
import type { SpecialtyDto } from '../types';
import { Spinner } from './Spinner';
import { ErrorMessage } from './ErrorMessage';

const ICONS: Record<string, string> = {
  cardiology: '❤️',
  dermatology: '🌿',
  pediatrics: '👶',
  neurology: '🧠',
  orthopedics: '🦴',
  dentistry: '🦷',
  ophthalmology: '👁️',
  gynecology: '🌸',
  surgery: '🔬',
  default: '🏥',
};

function icon(name: string): string {
  const key = name.toLowerCase();
  return ICONS[key] ?? ICONS.default;
}

interface Props {
  onSelect: (specialty: SpecialtyDto) => void;
}

export function SpecialtyStep({ onSelect }: Props) {
  const [specialties, setSpecialties] = useState<SpecialtyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = () => {
    setLoading(true);
    setError(null);
    getSpecialties()
      .then(setSpecialties)
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Failed to load specialties'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  if (loading) return <Spinner text="Loading specialties..." />;
  if (error) return <ErrorMessage message={error} onRetry={load} />;

  return (
    <div>
      <div className="step-intro">
        <h2>What brings you in?</h2>
        <p>Choose a medical specialty</p>
      </div>
      <div className="card-list">
        {specialties.map((s) => (
          <button key={s.id} className="card" onClick={() => onSelect(s)}>
            <span className="card-icon">{icon(s.name)}</span>
            <span className="card-body">
              <span className="card-title">{s.name}</span>
            </span>
            <span className="card-arrow">›</span>
          </button>
        ))}
      </div>
    </div>
  );
}
