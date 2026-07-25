// The doctor's login session. Unlike the one-time email tokens on the patient site, this is a
// durable JWT session, so it's kept in localStorage (cleared on logout or a 401).

const STORAGE_KEY = 'clinicsite.doctor.session';

export interface DoctorSession {
  token: string;
  doctorName: string;
  expiresAtUtc: string;
}

export function getSession(): DoctorSession | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;

  try {
    const session = JSON.parse(raw) as DoctorSession;
    if (!session.token || new Date(session.expiresAtUtc).getTime() <= Date.now()) {
      clearSession();
      return null;
    }
    return session;
  } catch {
    clearSession();
    return null;
  }
}

export function setSession(session: DoctorSession): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  localStorage.removeItem(STORAGE_KEY);
}
