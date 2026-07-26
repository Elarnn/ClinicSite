export const API_BASE_URL = 'https://localhost:7100/api';

async function toError(response: Response, fallback: string): Promise<Error> {
  const text = await response.text().catch(() => '');

  try {
    const parsed = JSON.parse(text) as { message?: string };
    if (parsed?.message) return new Error(parsed.message);
  } catch {
    // response body wasn't JSON, fall through to the generic message
  }

  return new Error(fallback);
}

export async function apiGet<T>(url: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`);

  if (!response.ok) {
    throw await toError(response, `GET ${url} failed: ${response.status}`);
  }

  return response.json();
}

export async function apiPost<TResponse, TBody>(
  url: string,
  body: TBody
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: body === null ? undefined : JSON.stringify(body),
  });

  if (!response.ok) {
    throw await toError(response, `POST ${url} failed: ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return response.json();
}

export async function apiPut<TResponse, TBody>(
  url: string,
  body: TBody
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw await toError(response, `PUT ${url} failed: ${response.status}`);
  }

  return response.json();
}

export async function apiDelete(url: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    throw await toError(response, `DELETE ${url} failed: ${response.status}`);
  }
}
