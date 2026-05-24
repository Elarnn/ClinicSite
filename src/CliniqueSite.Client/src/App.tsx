import { useEffect, useState } from "react";
import { getHealth, type HealthResponse } from "./api/health-api";
import "./App.css";

function App() {
  const [health, setHealth] = useState<HealthResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    getHealth()
      .then((data) => {
        setHealth(data);
      })
      .catch((error: unknown) => {
        if (error instanceof Error) {
          setError(error.message);
        } else {
          setError("Unknown error");
        }
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, []);

  if (isLoading) {
    return <main className="container">Loading API status...</main>;
  }

  if (error) {
    return (
      <main className="container">
        <h1>CliniqueSite</h1>
        <p className="error">Backend error: {error}</p>
      </main>
    );
  }

  return (
    <main className="container">
      <h1>CliniqueSite</h1>
      <p>Frontend is running.</p>

      <section className="card">
        <h2>Backend status</h2>
        <p>
          <strong>Status:</strong> {health?.status}
        </p>
        <p>
          <strong>Message:</strong> {health?.message}
        </p>
        <p>
          <strong>Timestamp:</strong> {health?.timestamp}
        </p>
      </section>
    </main>
  );
}

export default App;