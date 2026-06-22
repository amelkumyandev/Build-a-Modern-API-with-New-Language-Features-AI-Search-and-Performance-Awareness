"use client";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";
const tokenKey = "lki.accessToken";

export function getToken() {
  if (typeof window === "undefined") return null;
  return window.localStorage.getItem(tokenKey);
}

export function setToken(token: string) {
  window.localStorage.setItem(tokenKey, token);
}

export function clearToken() {
  window.localStorage.removeItem(tokenKey);
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers = new Headers(init.headers);
  const isFormData = typeof FormData !== "undefined" && init.body instanceof FormData;
  if (!headers.has("Content-Type") && !isFormData) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers,
    cache: "no-store"
  });

  if (!response.ok) {
    // An authenticated request was rejected: the token is expired or otherwise
    // invalid. Clear it and return to login so the user is not stranded on a
    // page that silently renders "down" defaults. We only do this when a token
    // was actually sent, so a bad-credentials 401 on the login form still shows
    // its own error instead of redirect-looping.
    if (response.status === 401 && token) {
      clearToken();
      if (typeof window !== "undefined" && window.location.pathname !== "/login") {
        window.location.assign("/login");
      }
      throw new Error("Your session has expired. Please sign in again.");
    }

    const text = await response.text();
    let message = text || `Request failed with ${response.status}`;
    try {
      const problem = JSON.parse(text) as { title?: string; detail?: string; errors?: Record<string, string[]> };
      const validation = problem.errors ? Object.values(problem.errors).flat().join(" ") : "";
      message = [problem.title, problem.detail, validation].filter(Boolean).join(" ") || message;
    } catch {
      // Keep the raw response body when it is not a ProblemDetails payload.
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
