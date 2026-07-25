import { useEffect, useState } from 'react';
import { clearSession, getSession } from './auth/session';
import type { DoctorSession } from './auth/session';
import { UNAUTHORIZED_EVENT } from './api/http';
import { LoginPage } from './pages/LoginPage';
import { SetPasswordPage } from './pages/SetPasswordPage';
import { BookingsPage } from './pages/BookingsPage';

export default function App() {
  const [session, setSession] = useState<DoctorSession | null>(getSession());
  const [path, setPath] = useState(window.location.pathname);

  useEffect(() => {
    const onPopState = () => setPath(window.location.pathname);
    const onUnauthorized = () => setSession(null);

    window.addEventListener('popstate', onPopState);
    window.addEventListener(UNAUTHORIZED_EVENT, onUnauthorized);
    return () => {
      window.removeEventListener('popstate', onPopState);
      window.removeEventListener(UNAUTHORIZED_EVENT, onUnauthorized);
    };
  }, []);

  function goToSignIn() {
    window.history.pushState({}, '', '/');
    setPath('/');
  }

  function handleLogout() {
    clearSession();
    setSession(null);
  }

  // The invite link lands here regardless of session state.
  if (path.startsWith('/set-password')) {
    return <SetPasswordPage onDone={goToSignIn} />;
  }

  if (!session) {
    return <LoginPage onLoggedIn={setSession} />;
  }

  return <BookingsPage session={session} onLogout={handleLogout} />;
}
