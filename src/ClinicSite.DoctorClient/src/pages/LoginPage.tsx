import { useState } from 'react';
import type { FormEvent } from 'react';
import { login } from '../api/auth';
import { ApiError } from '../api/http';
import { setSession } from '../auth/session';
import type { DoctorSession } from '../auth/session';

interface Props {
  onLoggedIn: (session: DoctorSession) => void;
}

export function LoginPage({ onLoggedIn }: Props) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (submitting) return;

    setSubmitting(true);
    setError(null);

    login(email.trim(), password)
      .then((result) => {
        const session: DoctorSession = {
          token: result.token,
          doctorName: result.doctorName,
          expiresAtUtc: result.expiresAtUtc,
        };
        setSession(session);
        onLoggedIn(session);
      })
      .catch((e: unknown) => {
        setError(e instanceof ApiError ? e.message : 'Login failed. Please try again.');
      })
      .finally(() => setSubmitting(false));
  }

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <div className="brand">ClinicSite <span>Doctor Portal</span></div>
        <h1 className="auth-title">Sign in</h1>
        <p className="auth-subtitle">Use the email and password for your doctor account.</p>

        <form onSubmit={handleSubmit} noValidate>
          <label className="field-label" htmlFor="email">Email</label>
          <input
            id="email"
            className="field-input"
            type="email"
            autoComplete="username"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={submitting}
          />

          <label className="field-label" htmlFor="password">Password</label>
          <input
            id="password"
            className="field-input"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={submitting}
          />

          {error && <p className="field-error">{error}</p>}

          <button className="btn-primary" type="submit" disabled={submitting || !email || !password}>
            {submitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </div>
    </div>
  );
}
