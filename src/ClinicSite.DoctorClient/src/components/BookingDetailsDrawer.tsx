import { useEffect, useState } from 'react';
import type { AppointmentStatus, PatientHistoryItem } from '../types';
import { getPatientHistory, sendPatientMessage, updateNote, updateStatus } from '../api/portal';
import { ApiError } from '../api/http';
import { STATUS_LABELS, fmtDateLong, fmtTime, statusClass } from '../lib/format';
import { BookingStatusActions } from './BookingStatusActions';
import { DoctorNoteEditor } from './DoctorNoteEditor';
import { PatientHistory } from './PatientHistory';
import { SendPatientMessageModal } from './SendPatientMessageModal';

export interface DrawerBooking {
  bookingId: string;
  patientName: string;
  patientEmail: string;
  patientComment: string | null;
  doctorNote: string | null;
  status: AppointmentStatus;
  startTimeUtc: string;
  endTimeUtc: string;
}

interface Props {
  booking: DrawerBooking;
  onClose: () => void;
  onChanged: () => void;
}

export function BookingDetailsDrawer({ booking, onClose, onChanged }: Props) {
  const [status, setStatus] = useState<AppointmentStatus>(booking.status);
  const [note, setNote] = useState<string | null>(booking.doctorNote);
  const [history, setHistory] = useState<PatientHistoryItem[] | null>(null);
  const [historyError, setHistoryError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [showMessage, setShowMessage] = useState(false);
  const [messageBusy, setMessageBusy] = useState(false);
  const [messageError, setMessageError] = useState<string | null>(null);
  const [messageSent, setMessageSent] = useState(false);

  useEffect(() => {
    setHistory(null);
    setHistoryError(null);
    getPatientHistory(booking.bookingId)
      .then(setHistory)
      .catch((e: unknown) => setHistoryError(e instanceof ApiError ? e.message : 'Failed to load history'));
  }, [booking.bookingId]);

  function changeStatus(next: AppointmentStatus) {
    setBusy(true);
    setError(null);
    updateStatus(booking.bookingId, next)
      .then(() => {
        setStatus(next);
        onChanged();
      })
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to update status'))
      .finally(() => setBusy(false));
  }

  function saveNote(text: string) {
    setBusy(true);
    setError(null);
    updateNote(booking.bookingId, text)
      .then((r) => {
        setNote(r.note);
        onChanged();
      })
      .catch((e: unknown) => setError(e instanceof ApiError ? e.message : 'Failed to save note'))
      .finally(() => setBusy(false));
  }

  function sendMessage(subject: string, message: string) {
    setMessageBusy(true);
    setMessageError(null);
    sendPatientMessage(booking.bookingId, subject, message)
      .then(() => {
        setMessageSent(true);
        setShowMessage(false);
      })
      .catch((e: unknown) => setMessageError(e instanceof ApiError ? e.message : 'Failed to send message'))
      .finally(() => setMessageBusy(false));
  }

  return (
    <div className="drawer-backdrop" onClick={onClose}>
      <aside className="drawer" onClick={(e) => e.stopPropagation()}>
        <div className="drawer-head">
          <h2>{booking.patientName}</h2>
          <button className="drawer-close" onClick={onClose} aria-label="Close">
            ×
          </button>
        </div>

        <div className="drawer-body">
          <section>
            <div className="kv"><span>Email</span><strong>{booking.patientEmail}</strong></div>
            <div className="kv"><span>Date</span><strong>{fmtDateLong(booking.startTimeUtc)}</strong></div>
            <div className="kv">
              <span>Time</span>
              <strong>{fmtTime(booking.startTimeUtc)} – {fmtTime(booking.endTimeUtc)}</strong>
            </div>
            <div className="kv"><span>Status</span><span className={statusClass(status)}>{STATUS_LABELS[status]}</span></div>
          </section>

          <section>
            <h4>Update status</h4>
            <BookingStatusActions current={status} busy={busy} onChange={changeStatus} />
          </section>

          <section>
            <h4>Patient comment</h4>
            <p className="readonly-block">{booking.patientComment ?? '—'}</p>
          </section>

          <section>
            <h4>Doctor note</h4>
            <DoctorNoteEditor note={note} busy={busy} onSave={saveNote} />
          </section>

          <section>
            <div className="section-head">
              <h4>Message patient</h4>
              {messageSent && <span className="muted">Sent ✓</span>}
            </div>
            <button
              className="btn-ghost"
              onClick={() => {
                setMessageError(null);
                setShowMessage(true);
              }}
            >
              Compose message…
            </button>
          </section>

          <section>
            <h4>Patient history</h4>
            <PatientHistory items={history} error={historyError} currentBookingId={booking.bookingId} />
          </section>

          {error && <p className="field-error">{error}</p>}
        </div>
      </aside>

      {showMessage && (
        <SendPatientMessageModal
          patientName={booking.patientName}
          busy={messageBusy}
          error={messageError}
          onClose={() => setShowMessage(false)}
          onSend={sendMessage}
        />
      )}
    </div>
  );
}
