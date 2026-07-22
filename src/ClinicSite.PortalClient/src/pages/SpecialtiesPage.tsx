import { useEffect, useState } from 'react';
import { createSpecialty, deleteSpecialty, getSpecialties, updateSpecialty } from '../api/specialties';
import type { SpecialtyDto } from '../types';

export function SpecialtiesPage() {
  const [specialties, setSpecialties] = useState<SpecialtyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [newName, setNewName] = useState('');
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState('');
  const [busyId, setBusyId] = useState<string | null>(null);
  const [rowError, setRowError] = useState<string | null>(null);

  useEffect(() => {
    getSpecialties()
      .then(setSpecialties)
      .catch((e: unknown) => {
        setError(e instanceof Error ? e.message : 'Failed to load specialties');
      })
      .finally(() => setLoading(false));
  }, []);

  function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    const name = newName.trim();
    if (!name || creating) return;

    setCreating(true);
    setCreateError(null);
    createSpecialty({ name })
      .then((created) => {
        setSpecialties((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
        setNewName('');
        setShowCreateForm(false);
      })
      .catch((e: unknown) => {
        setCreateError(e instanceof Error ? e.message : 'Failed to create specialty');
      })
      .finally(() => setCreating(false));
  }

  function startEdit(specialty: SpecialtyDto) {
    setEditingId(specialty.id);
    setEditName(specialty.name);
    setRowError(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setEditName('');
  }

  function saveEdit(id: string) {
    const name = editName.trim();
    if (!name) return;

    setBusyId(id);
    setRowError(null);
    updateSpecialty(id, { name })
      .then((updated) => {
        setSpecialties((prev) =>
          prev.map((s) => (s.id === id ? updated : s)).sort((a, b) => a.name.localeCompare(b.name))
        );
        setEditingId(null);
      })
      .catch((e: unknown) => {
        setRowError(e instanceof Error ? e.message : 'Failed to update specialty');
      })
      .finally(() => setBusyId(null));
  }

  function handleDelete(specialty: SpecialtyDto) {
    if (!window.confirm(`Delete specialty "${specialty.name}"?`)) return;

    setBusyId(specialty.id);
    setRowError(null);
    deleteSpecialty(specialty.id)
      .then(() => {
        setSpecialties((prev) => prev.filter((s) => s.id !== specialty.id));
      })
      .catch((e: unknown) => {
        setRowError(e instanceof Error ? e.message : 'Failed to delete specialty');
      })
      .finally(() => setBusyId(null));
  }

  if (loading) return <p>Loading specialties...</p>;
  if (error) return <p className="error-text">{error}</p>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Specialties</h1>
          <p>Manage clinic specialties.</p>
        </div>

        <button className="primary-button" onClick={() => setShowCreateForm((v) => !v)}>
          {showCreateForm ? 'Cancel' : 'Add specialty'}
        </button>
      </div>

      {showCreateForm && (
        <form className="inline-form" onSubmit={handleCreate}>
          <input
            className="text-input"
            placeholder="Specialty name"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            disabled={creating}
            autoFocus
          />
          <button className="primary-button" type="submit" disabled={creating || !newName.trim()}>
            {creating ? 'Saving…' : 'Save'}
          </button>
        </form>
      )}
      {createError && <p className="error-text">{createError}</p>}
      {rowError && <p className="error-text">{rowError}</p>}

      {specialties.length === 0 ? (
        <div className="empty-state">
          <h2>No specialties yet</h2>
          <p>Add a specialty to start assigning doctors to it.</p>
        </div>
      ) : (
        <div className="table-scroll">
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
                  {editingId === specialty.id ? (
                    <>
                      <td>
                        <input
                          className="text-input"
                          value={editName}
                          onChange={(e) => setEditName(e.target.value)}
                          disabled={busyId === specialty.id}
                          autoFocus
                        />
                      </td>
                      <td className="table-actions">
                        <button
                          className="small-button"
                          onClick={() => saveEdit(specialty.id)}
                          disabled={busyId === specialty.id || !editName.trim()}
                        >
                          Save
                        </button>
                        <button
                          className="small-button"
                          onClick={cancelEdit}
                          disabled={busyId === specialty.id}
                        >
                          Cancel
                        </button>
                      </td>
                    </>
                  ) : (
                    <>
                      <td>{specialty.name}</td>
                      <td className="table-actions">
                        <button
                          className="small-button"
                          onClick={() => startEdit(specialty)}
                          disabled={busyId === specialty.id}
                        >
                          Edit
                        </button>
                        <button
                          className="small-button danger"
                          onClick={() => handleDelete(specialty)}
                          disabled={busyId === specialty.id}
                        >
                          {busyId === specialty.id ? '…' : 'Delete'}
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
