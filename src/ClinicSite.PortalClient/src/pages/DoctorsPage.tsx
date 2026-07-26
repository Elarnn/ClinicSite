import { useEffect, useState } from 'react';
import {
  activateDoctor,
  createDoctor,
  deactivateDoctor,
  deleteDoctor,
  doctorPhotoUrl,
  getAdminDoctors,
  inviteDoctor,
  removeDoctorPhoto,
  updateDoctor,
  uploadDoctorPhoto,
} from '../api/doctors';
import { getSpecialties } from '../api/specialties';
import type { DoctorDto, SpecialtyDto } from '../types';

function initials(fullName: string): string {
  return fullName
    .split(' ')
    .filter(Boolean)
    .map((w) => w[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

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
  // Bumped after a photo upload/remove so thumbnails bypass the browser cache.
  const [photoVersion, setPhotoVersion] = useState(0);

  // Account invitation (bind an email + send the set-password link).
  const [inviteId, setInviteId] = useState<string | null>(null);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteBusy, setInviteBusy] = useState(false);
  const [inviteError, setInviteError] = useState<string | null>(null);

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

  function startInvite(doctor: DoctorDto) {
    setInviteId(doctor.id);
    setInviteEmail(doctor.email ?? '');
    setInviteError(null);
  }

  function cancelInvite() {
    setInviteId(null);
    setInviteEmail('');
    setInviteError(null);
  }

  function submitInvite(doctor: DoctorDto) {
    const email = inviteEmail.trim();
    if (!email || inviteBusy) return;

    setInviteBusy(true);
    setInviteError(null);
    inviteDoctor(doctor.id, email)
      .then((updated) => {
        setDoctors((prev) => prev.map((d) => (d.id === doctor.id ? updated : d)));
        cancelInvite();
      })
      .catch((e: unknown) => {
        setInviteError(e instanceof Error ? e.message : 'Failed to send invitation');
      })
      .finally(() => setInviteBusy(false));
  }

  function handlePhotoSelected(doctor: DoctorDto, e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    e.target.value = ''; // allow re-selecting the same file later
    if (!file) return;

    setBusyId(doctor.id);
    setRowError(null);
    uploadDoctorPhoto(doctor.id, file)
      .then(() => {
        setDoctors((prev) => prev.map((d) => (d.id === doctor.id ? { ...d, hasPhoto: true } : d)));
        setPhotoVersion((v) => v + 1);
      })
      .catch((err: unknown) => setRowError(err instanceof Error ? err.message : 'Failed to upload photo'))
      .finally(() => setBusyId(null));
  }

  function handleRemovePhoto(doctor: DoctorDto) {
    if (!window.confirm(`Remove ${doctor.fullName}'s photo?`)) return;

    setBusyId(doctor.id);
    setRowError(null);
    removeDoctorPhoto(doctor.id)
      .then(() => {
        setDoctors((prev) => prev.map((d) => (d.id === doctor.id ? { ...d, hasPhoto: false } : d)));
        setPhotoVersion((v) => v + 1);
      })
      .catch((err: unknown) => setRowError(err instanceof Error ? err.message : 'Failed to remove photo'))
      .finally(() => setBusyId(null));
  }

  function photoCell(doctor: DoctorDto, controls: boolean) {
    return (
      <td>
        <div className="photo-cell">
          {doctor.hasPhoto ? (
            <img className="photo-thumb" src={`${doctorPhotoUrl(doctor.id)}?v=${photoVersion}`} alt="" />
          ) : (
            <span className="photo-thumb placeholder">{initials(doctor.fullName)}</span>
          )}
          {controls && (
            <div className="photo-actions">
              <label className={`small-button${busyId === doctor.id ? ' disabled' : ''}`}>
                {doctor.hasPhoto ? 'Change' : 'Upload'}
                <input
                  type="file"
                  accept="image/*"
                  hidden
                  disabled={busyId === doctor.id}
                  onChange={(e) => handlePhotoSelected(doctor, e)}
                />
              </label>
              {doctor.hasPhoto && (
                <button
                  className="small-button danger"
                  disabled={busyId === doctor.id}
                  onClick={() => handleRemovePhoto(doctor)}
                >
                  Remove
                </button>
              )}
            </div>
          )}
        </div>
      </td>
    );
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
                <th>Photo</th>
                <th>Full name</th>
                <th>Specialty</th>
                <th>Status</th>
                <th>Account</th>
                <th className="table-actions">Actions</th>
              </tr>
            </thead>

            <tbody>
              {doctors.map((doctor) => (
                <tr key={doctor.id}>
                  {editingId === doctor.id ? (
                    <>
                      {photoCell(doctor, false)}
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
                      <td>{doctor.accountStatus === 'None' ? 'No account' : doctor.accountStatus}</td>
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
                      {photoCell(doctor, true)}
                      <td>{doctor.fullName}</td>
                      <td>{doctor.specialtyName}</td>
                      <td>{doctor.isActive ? 'Active' : 'Inactive'}</td>
                      <td>
                        {inviteId === doctor.id ? (
                          <div className="invite-editor">
                            <input
                              className="text-input"
                              type="email"
                              placeholder="doctor@email.com"
                              value={inviteEmail}
                              onChange={(e) => setInviteEmail(e.target.value)}
                              disabled={inviteBusy}
                              autoFocus
                            />
                            <div className="invite-editor-actions">
                              <button
                                className="small-button"
                                onClick={() => submitInvite(doctor)}
                                disabled={inviteBusy || !inviteEmail.trim()}
                              >
                                {inviteBusy ? '…' : 'Send'}
                              </button>
                              <button className="small-button" onClick={cancelInvite} disabled={inviteBusy}>
                                Cancel
                              </button>
                            </div>
                            {inviteError && <p className="error-text">{inviteError}</p>}
                          </div>
                        ) : doctor.accountStatus === 'Active' ? (
                          <div>
                            <span className="account-badge active">Active</span>
                            <div className="account-email">{doctor.email}</div>
                          </div>
                        ) : doctor.accountStatus === 'Invited' ? (
                          <div>
                            <span className="account-badge invited">Invited</span>
                            <div className="account-email">{doctor.email}</div>
                            <button className="small-button" onClick={() => startInvite(doctor)}>
                              Resend
                            </button>
                          </div>
                        ) : (
                          <div>
                            <span className="account-badge none">No account</span>
                            <button className="small-button" onClick={() => startInvite(doctor)}>
                              Invite
                            </button>
                          </div>
                        )}
                      </td>
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
