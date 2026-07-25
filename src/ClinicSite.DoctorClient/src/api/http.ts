import { clearSession, getSession } from '../auth/session';

const API_BASE_URL = 'https://localhost:7100/api';

/** An error carrying the HTTP status so pages can distinguish 410 (expired) / 401 (auth) etc. */
export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.status = status;
    this.name = 'ApiError';
  }
}

/** Fired when the API rejects our token; the App listens and drops the session. */
export const UNAUTHORIZED_EVENT = 'doctor-unauthorized';

async function toError(response: Response, fallback: string): Promise<ApiError> {
  const text = await response.text().catch(() => '');
  let message = fallback;
  try {
    const parsed = JSON.parse(text) as { message?: string };
    if (parsed?.message) message = parsed.message;
  } catch {
    // body wasn't JSON — keep the fallback message
  }
  return new ApiError(message, response.status);
}

function authHeaders(): Record<string, string> {
  const session = getSession();
  return session ? { Authorization: `Bearer ${session.token}` } : {};
}

async function handle<T>(response: Response, fallback: string): Promise<T> {
  if (response.status === 401) {
    clearSession();
    window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
    throw new ApiError('Your session has expired. Please sign in again.', 401);
  }
  if (!response.ok) {
    throw await toError(response, fallback);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return response.json() as Promise<T>;
}

export async function apiGet<T>(url: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    headers: { ...authHeaders() },
  });
  return handle<T>(response, `GET ${url} failed: ${response.status}`);
}

export async function apiPost<TResponse, TBody>(url: string, body: TBody): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: body === null ? undefined : JSON.stringify(body),
  });
  return handle<TResponse>(response, `POST ${url} failed: ${response.status}`);
}
