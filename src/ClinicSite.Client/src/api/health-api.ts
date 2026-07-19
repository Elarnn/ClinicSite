export type HealthResponse = {
  status: string;
  message: string;
  timestamp: string;
};

const API_BASE_URL = "https://localhost:32769";

export async function getHealth(): Promise<HealthResponse> {
  const response = await fetch(`${API_BASE_URL}/api/health`);

  if (!response.ok) {
    throw new Error("Failed to fetch API health status");
  }

  return response.json();
}