import { useEffect, useState } from 'react';
import { activateDoctor, createDoctor, deactivateDoctor, deleteDoctor, getAdminDoctors, updateDoctor } from '../api/doctors';
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

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editFullName, setEditFullName] = useState('');
  const [editSpecialtyId, setEditSpecialtyId] = useState('');
  const [busyId, setBusyId] = useState<string | null>(null);
  const [rowError, setRowError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([getAdminDoctors(), getSpecialties()])
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

  function startEdit(doctor: DoctorDto) {
    setEditingId(doctor.id);
    setEditFullName(doctor.fullName);
    setEditSpecialtyId(doctor.specialtyId);
    setRowError(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setEditFullName('');
    setEditSpecialtyId('');
  }

  function saveEdit(id: string) {
    const name = editFullName.trim();
    if (!name || !editSpecialtyId) return;

    setBusyId(id);
    setRowError(null);
    updateDoctor(id, { fullName: name, specialtyId: editSpecialtyId })
      .then((updated) => {
        setDoctors((prev) => prev.map((d) => (d.id === id ? updated : d)));
        setEditingId(null);
      })
      .catch((e: unknown) => {
        setRowError(e instanceof Error ? e.message : 'Failed to update doctor');
      })
      .finally(() => setBusyId(null));
  }

  function handleToggleActive(doctor: DoctorDto) {
    const action = doctor.isActive ? 'Deactivate' : 'Activate';
    if (!window.confirm(`${action} doctor "${doctor.fullName}"?`)) return;

    setBusyId(doctor.id);
    setRowError(null);
    const request = doctor.isActive ? deactivateDoctor(doctor.id) : activateDoctor(doctor.id);
    request
      .then((updated) => {
        setDoctors((prev) => prev.map((d) => (d.id === doctor.id ? updated : d)));
      })
      .catch((e: unknown) => {
        setRowError(e instanceof Error ? e.message : `Failed to ${action.toLowerCase()} doctor`);
      })
      .finally(() => setBusyId(null));
  }

  function handleDelete(doctor: DoctorDto) {
    if (!window.confirm(`Permanently delete doctor "${doctor.fullName}"? This cannot be undone.`)) return;

    setBusyId(doctor.id);
    setRowError(null);
    deleteDoctor(doctor.id)
      .then(() => {
        setDoctors((prev) => prev.filter((d) => d.id !== doctor.id));
      })
      .catch((e: unknown) => {
        setRowError(e instanceof Error ? e.message : 'Failed to delete doctor');
      })
      .finally(() => setBusyId(null));
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
      {rowError && <p className="error-text">{rowError}</p>}

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
                <th>Status</th>
                <th className="table-actions">Actions</th>
              </tr>
            </thead>

            <tbody>
              {doctors.map((doctor) => (
                <tr key={doctor.id}>
                  {editingId === doctor.id ? (
                    <>
                      <td>
                        <input
                          className="text-input"
                          value={editFullName}
                          onChange={(e) => setEditFullName(e.target.value)}
                          disabled={busyId === doctor.id}
                          autoFocus
                        />
                      </td>
                      <td>
                        <select
                          className="text-input"
                          value={editSpecialtyId}
                          onChange={(e) => setEditSpecialtyId(e.target.value)}
                          disabled={busyId === doctor.id}
                        >
                          {specialties.map((s) => (
                            <option key={s.id} value={s.id}>
                              {s.name}
                            </option>
                          ))}
                        </select>
                      </td>
                      <td>{doctor.isActive ? 'Active' : 'Inactive'}</td>
                      <td className="table-actions">
                        <button
                          className="small-button"
                          onClick={() => saveEdit(doctor.id)}
                          disabled={busyId === doctor.id || !editFullName.trim()}
                        >
                          Save
                        </button>
                        <button
                          className="small-button"
                          onClick={cancelEdit}
                          disabled={busyId === doctor.id}
                        >
                          Cancel
                        </button>
                      </td>
                    </>
                  ) : (
                    <>
                      <td>{doctor.fullName}</td>
                      <td>{doctor.specialtyName}</td>
                      <td>{doctor.isActive ? 'Active' : 'Inactive'}</td>
                      <td className="table-actions">
                        <button
                          className="small-button"
                          onClick={() => startEdit(doctor)}
                          disabled={busyId === doctor.id}
                        >
                          Edit
                        </button>
                        <button
                          className="small-button"
                          onClick={() => handleToggleActive(doctor)}
                          disabled={busyId === doctor.id}
                        >
                          {busyId === doctor.id ? '…' : doctor.isActive ? 'Deactivate' : 'Activate'}
                        </button>
                        <button
                          className="small-button danger"
                          onClick={() => handleDelete(doctor)}
                          disabled={busyId === doctor.id}
                        >
                          Delete
                        </button>
                      </td>
                    </>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
