import { APIRequestContext, APIResponse } from '@playwright/test';
import { throttleAuth } from './authThrottle';

const AUTH_WINDOW_MS = 31_000;

/**
 * POST vers un endpoint /api/auth/* : throttle proactif (respecte 5/30s) + filet de
 * sécurité réactif (si un 429 passe quand même, on attend la fenêtre et on réessaie).
 * Garantit que les flux fonctionnels ne sont jamais faussés par le rate-limiter.
 */
export async function postAuth(
  api: APIRequestContext,
  url: string,
  data: unknown,
  retries = 2,
): Promise<APIResponse> {
  await throttleAuth();
  let res = await api.post(url, { data });
  while (res.status() === 429 && retries > 0) {
    await new Promise((r) => setTimeout(r, AUTH_WINDOW_MS));
    await throttleAuth();
    res = await api.post(url, { data });
    retries -= 1;
  }
  return res;
}
