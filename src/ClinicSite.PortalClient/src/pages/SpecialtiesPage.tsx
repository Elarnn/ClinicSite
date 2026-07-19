import { useEffect, useState } from 'react';
import { getSpecialties } from '../api/specialties';
import type { SpecialtyDto } from '../types';

export function SpecialtiesPage() {
  const [specialties, setSpecialties] = useState<SpecialtyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getSpecialties()
      .then(setSpecialties)
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'Failed to load specialties');
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading specialties...</p>;
  if (error) return <p className="error-text">{error}</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Specialties</h1>
          <p>Manage clinic specialties.</p>
        </div>

        <button className="primary-button">Add specialty</button>
      </div>

      <table className="data-table">
        <thead>
          <tr>
            <th>Name</th>
            <th className="table-actions">Actions</th>
          </tr>
        </thead>

        <tbody>
          {specialties.map((specialty) => (
            <tr key={specialty.id}>
              <td>{specialty.name}</td>
              <td className="table-actions">
                <button className="small-button">Edit</button>
                <button className="small-button danger">Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}