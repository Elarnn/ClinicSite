import { useEffect, useState } from 'react';
import { createDoctor, getDoctors } from '../api/doctors';
import { getSpecialties } from '../api/specialties';
import type { DoctorDto, SpecialtyDto } from '../types';

export function DoctorsPage() {
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [specialties, setSpecialties] = useState<SpecialtyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [fullName, setFullName] = useState('');
  const [specialtyId, setSpecialtyId] = useState('');
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([getDoctors(), getSpecialties()])
      .then(([doctorList, specialtyList]) => {
        setDoctors(doctorList);
        setSpecialties(specialtyList);
        setSpecialtyId(specialtyList[0]?.id ?? '');
      })
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'Failed to load doctors');
      })
      .finally(() => setLoading(false));
  }, []);

  function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    const name = fullName.trim();
    if (!name || !specialtyId || creating) return;

    setCreating(true);
    setCreateError(null);
    createDoctor({ fullName: name, specialtyId })
      .then((created) => {
        setDoctors((prev) => [...prev, created]);
        setFullName('');
        setShowCreateForm(false);
      })
      .catch((e: unknown) => {
        setCreateError(e instanceof Error ? e.message : 'Failed to create doctor');
      })
      .finally(() => setCreating(false));
  }

  if (loading) return <p>Loading doctors...</p>;
  if (error) return <p className="error-text">{error}</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Doctors</h1>
          <p>Manage clinic doctors.</p>
        </div>

        {specialties.length > 0 && (
          <button className="primary-button" onClick={() => setShowCreateForm((v) => !v)}>
            {showCreateForm ? 'Cancel' : 'Add doctor'}
          </button>
        )}
      </div>

      {specialties.length === 0 && (
        <div className="empty-state">
          <h2>No specialties yet</h2>
          <p>Add a specialty first before creating doctors.</p>
        </div>
      )}

      {showCreateForm && (
        <form className="inline-form" onSubmit={handleCreate}>
          <input
            className="text-input"
            placeholder="Doctor full name"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            disabled={creating}
            autoFocus
          />
          <select
            className="text-input"
            value={specialtyId}
            onChange={(e) => setSpecialtyId(e.target.value)}
            disabled={creating}
          >
            {specialties.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </select>
          <button className="primary-button" type="submit" disabled={creating || !fullName.trim()}>
            {creating ? 'Saving…' : 'Save'}
          </button>
        </form>
      )}
      {createError && <p className="error-text">{createError}</p>}

      {doctors.length === 0 ? (
        <div className="empty-state">
          <h2>No doctors yet</h2>
          <p>Doctors you add will appear here.</p>
        </div>
      ) : (
        <div className="table-scroll">
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
      )}
    </div>
  );
}
