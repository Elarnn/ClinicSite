import { useEffect, useState } from 'react';
import { getDoctors } from '../api/doctors';
import type { DoctorDto } from '../types';

export function DoctorsPage() {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getDoctors()
      .then(setDoctors)
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'Failed to load doctors');
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Loading doctors...</p>;
  if (error) return <p className="error-text">{error}</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Doctors</h1>
          <p>Manage clinic doctors.</p>
        </div>

        <button className="primary-button">Add doctor</button>
      </div>

      <table className="data-table">
        <thead>
          <tr>
            <th>Full name</th>
            <th>Specialty</th>
            <th className="table-actions">Actions</th>
          </tr>
        </thead>

        <tbody>
          {doctors.map((doctor) => (
            <tr key={doctor.id}>
              <td>{doctor.fullName}</td>
              <td>{doctor.specialtyName}</td>
              <td className="table-actions">
                <button className="small-button">Edit</button>
                <button className="small-button danger">Deactivate</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}