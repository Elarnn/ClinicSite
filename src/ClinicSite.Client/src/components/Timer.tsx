import { useEffect, useState } from 'react';

interface TimerProps {
  reservedUntilUtc: string;
  onExpired: () => void;
}

export function Timer({ reservedUntilUtc, onExpired }: TimerProps) {
  // null = not yet calculated (prevents false onExpired on mount)
  const [secondsLeft, setSecondsLeft] = useState<number | null>(null);

  useEffect(() => {
    const calc = () =>
      Math.max(0, Math.floor((new Date(reservedUntilUtc).getTime() - Date.now()) / 1000));

    const initial = calc();
    setSecondsLeft(initial);

    if (initial === 0) return;

    const id = setInterval(() => {
      const s = calc();
      setSecondsLeft(s);
      if (s === 0) clearInterval(id);
    }, 1000);

    return () => clearInterval(id);
  }, [reservedUntilUtc]);

  useEffect(() => {
    if (secondsLeft === 0) onExpired();
  }, [secondsLeft, onExpired]);

  if (secondsLeft === null) return null;

  const mm = String(Math.floor(secondsLeft / 60)).padStart(2, '0');
  const ss = String(secondsLeft % 60).padStart(2, '0');
  const isExpiring = secondsLeft > 0 && secondsLeft <= 60;

  return (
    <div className={`timer-banner${isExpiring ? ' expiring' : ''}`}>
      <span className="timer-icon">⏱</span>
      <span className="timer-text">Slot reserved for</span>
      <span className="timer-countdown">{mm}:{ss}</span>
    </div>
  );
}
