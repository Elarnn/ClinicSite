import { useState } from 'react';

interface Props {
  patientName: string;
  busy: boolean;
  error: string | null;
  onClose: () => void;
  onSend: (subject: string, message: string) => void;
}

const TEMPLATES: { name: string; subject: string; body: (name: string) => string }[] = [
  {
    name: 'Appointment reminder',
    subject: 'Appointment reminder',
    body: (n) => `Hello ${n},\n\nThis is a reminder about your upcoming appointment. See you soon!`,
  },
  {
    name: 'Please come earlier',
    subject: 'Please come earlier',
    body: (n) => `Hello ${n},\n\nIf possible, could you please arrive a little earlier for your appointment? Thank you.`,
  },
  {
    name: 'Running late / delay',
    subject: 'Small delay',
    body: (n) => `Hello ${n},\n\nYour appointment may be delayed slightly. Thank you for your patience.`,
  },
  {
    name: 'Appointment time change',
    subject: 'Appointment time change',
    body: (n) => `Hello ${n},\n\nWe need to change the time of your appointment. Please contact the clinic to arrange a new time.`,
  },
];

export function SendPatientMessageModal({ patientName, busy, error, onClose, onSend }: Props) {
  const [subject, setSubject] = useState('');
  const [message, setMessage] = useState('');

  function applyTemplate(index: number) {
    const t = TEMPLATES[index];
    if (!t) return;
    setSubject(t.subject);
    setMessage(t.body(patientName));
  }

  return (
    <div
      className="modal-backdrop"
      onClick={(e) => {
        e.stopPropagation();
        onClose();
      }}
    >
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h3>Message {patientName}</h3>

        <label className="field-label">Template</label>
        <select
          className="field-input"
          defaultValue=""
          onChange={(e) => applyTemplate(Number(e.target.value))}
          disabled={busy}
        >
          <option value="" disabled>
            Choose a template…
          </option>
          {TEMPLATES.map((t, i) => (
            <option key={i} value={i}>
              {t.name}
            </option>
          ))}
        </select>

        <label className="field-label">Subject</label>
        <input
          className="field-input"
          maxLength={200}
          value={subject}
          onChange={(e) => setSubject(e.target.value)}
          disabled={busy}
        />

        <label className="field-label">Message</label>
        <textarea
          className="field-input"
          rows={5}
          maxLength={4000}
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          disabled={busy}
        />

        {error && <p className="field-error">{error}</p>}

        <div className="modal-actions">
          <button className="btn-ghost" onClick={onClose} disabled={busy}>
            Cancel
          </button>
          <button
            className="btn-primary"
            disabled={busy || !subject.trim() || !message.trim()}
            onClick={() => onSend(subject.trim(), message.trim())}
          >
            {busy ? 'Sending…' : 'Send'}
          </button>
        </div>
      </div>
    </div>
  );
}
