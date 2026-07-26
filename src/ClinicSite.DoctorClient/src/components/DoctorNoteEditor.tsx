import { useEffect, useState } from 'react';

interface Props {
  note: string | null;
  busy: boolean;
  onSave: (note: string) => void;
}

export function DoctorNoteEditor({ note, busy, onSave }: Props) {
  const [value, setValue] = useState(note ?? '');

  // Re-sync when the saved note changes (e.g. after a successful save / switching bookings).
  useEffect(() => {
    setValue(note ?? '');
  }, [note]);

  const dirty = (note ?? '') !== value;

  return (
    <div className="note-editor">
      <textarea
        className="note-textarea"
        rows={4}
        maxLength={2000}
        value={value}
        placeholder="Private note — the patient can't see this…"
        onChange={(e) => setValue(e.target.value)}
        disabled={busy}
      />
      <div className="note-footer">
        <span className="muted">{value.length}/2000</span>
        <button className="btn-primary small" disabled={busy || !dirty} onClick={() => onSave(value)}>
          {busy ? 'Saving…' : 'Save note'}
        </button>
      </div>
    </div>
  );
}
