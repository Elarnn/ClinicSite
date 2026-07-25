import { useEffect, useRef, useState } from 'react';
import type { FormEvent } from 'react';
import { getInviteInfo, setPassword as setPasswordApi } from '../api/auth';
import { ApiError } from '../api/http';
import type { DoctorInviteInfo } from '../types';

type State =
  | { kind: 'loading' }
  | { kind: 'form'; info: DoctorInviteInfo }
  | { kind: 'saving'; info: DoctorInviteInfo }
  | { kind: 'done'; info: DoctorInviteInfo }
  | { kind: 'expired' }
  | { kind: 'invalid' };

interface Props {
  onDone: () => void;
}

export function SetPasswordPage({ onDone }: Props) {
  const [state, setState] = useState<State>({ kind: 'loading' });
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState<string | null>(null);
  // Token is held in memory only — never in storage, never rendered.
  const tokenRef = useRef<string | null>(null);
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const token = new URLSearchParams(window.location.search).get('token');
    tokenRef.current = token;
    // Strip the token from the address bar immediately.
    window.history.replaceState({}, '', '/set-password');

    if (!token) {
      setState({ kind: 'invalid' });
      return;
    }

    getInviteInfo(token)
      .then((info) => setState({ kind: 'form', info }))
      .catch((e: unknown) => {
        setState(e instanceof ApiError && e.status === 410 ? { kind: 'expired' } : { kind: 'invalid' });
      });
  }, []);

  function handleSubmit(e: FormEvent, info: DoctorInviteInfo) {
    e.preventDefault();
    setError(null);

    if (password.length < 8) {
      setError('Password must be at least 8 characters.');
      return;
    }
    if (password !== confirm) {
      setError('Passwords do not match.');
      return;
    }

    const token = tokenRef.current;
    if (!token) {
      setState({ kind: 'invalid' });
      return;
    }

    setState({ kind: 'saving', info });
    setPasswordApi(token, password)
      .then(() => setState({ kind: 'done', info }))
      .catch((e: unknown) => {
        if (e instanceof ApiError && e.status === 410) {
          setState({ kind: 'expired' });
        } else {
          setError(e instanceof ApiError ? e.message : 'Could not set the password.');
          setState({ kind: 'form', info });
        }
      });
  }

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <div className="brand">ClinicSite <span>Doctor Portal</span></div>

        {state.kind === 'loading' && <p className="auth-subtitle">Checking your invitation…</p>}

        {(state.kind === 'form' || state.kind === 'saving') && (
          <>
            <h1 className="auth-title">Welcome, {state.info.doctorName}</h1>
            <p className="auth-subtitle">Set a password for <strong>{state.info.email}</strong> to activate your account.</p>

            <form onSubmit={(e) => handleSubmit(e, state.info)} noValidate>
              <label className="field-label" htmlFor="password">New password</label>
              <input
                id="password"
                className="field-input"
                type="password"
                autoComplete="new-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={state.kind === 'saving'}
              />

              <label className="field-label" htmlFor="confirm">Confirm password</label>
              <input
                id="confirm"
                className="field-input"
                type="password"
                autoComplete="new-password"
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                disabled={state.kind === 'saving'}
              />

              {error && <p className="field-error">{error}</p>}

              <button className="btn-primary" type="submit" disabled={state.kind === 'saving'}>
                {state.kind === 'saving' ? 'Saving…' : 'Set password'}
              </button>
            </form>
          </>
        )}

        {state.kind === 'done' && (
          <>
            <div className="status-icon success">✅</div>
            <h1 className="auth-title">Account activated</h1>
            <p className="auth-subtitle">Your password is set. You can now sign in.</p>
            <button className="btn-primary" onClick={onDone}>Go to sign in</button>
          </>
        )}

        {state.kind === 'expired' && (
          <>
            <div className="status-icon warning">⏰</div>
            <h1 className="auth-title">Invitation expired</h1>
            <p className="auth-subtitle">This invitation link is no longer valid. Ask the clinic to send a new one.</p>
          </>
        )}

        {state.kind === 'invalid' && (
          <>
            <div className="status-icon error">⚠️</div>
            <h1 className="auth-title">Invalid link</h1>
            <p className="auth-subtitle">This link is invalid or has already been used.</p>
          </>
        )}
      </div>
    </div>
  );
}
