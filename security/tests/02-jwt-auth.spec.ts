import { test, expect } from './helpers/fixtures';
import { API_BASE } from './helpers/env';

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

function decodePayload(token: string): Record<string, unknown> {
  return JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString());
}

function b64url(obj: object): string {
  return Buffer.from(JSON.stringify(obj)).toString('base64url');
}

/** En-tête réécrit en alg:none, signature vidée. */
function forgeAlgNone(token: string): string {
  const [, payload] = token.split('.');
  return `${b64url({ alg: 'none', typ: 'JWT' })}.${payload}.`;
}

/** Dernier caractère de la signature modifié. */
function tamperSignature(token: string): string {
  const [h, p, s] = token.split('.');
  const flipped = s.slice(0, -1) + (s.slice(-1) === 'A' ? 'B' : 'A');
  return `${h}.${p}.${flipped}`;
}

/** Payload modifié pour se donner le rôle Admin, en gardant l'ancienne signature. */
function forgeAdminRole(token: string): string {
  const [h, , s] = token.split('.');
  const payload = decodePayload(token);
  payload[ROLE_CLAIM] = 'Admin';
  return `${h}.${b64url(payload)}.${s}`;
}

test.describe('Sécurité — JWT', () => {
  test('endpoint protégé refuse une requête sans token', async ({ playwright }) => {
    const anon = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await anon.get('/api/candidatures');
    expect(res.status(), 'sans token → 401').toBe(401);
    await anon.dispose();
  });

  test('un access token valide passe (contrôle positif)', async ({ playwright, users }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.get('/api/candidatures', {
      headers: { Authorization: `Bearer ${users.a.accessToken}` },
    });
    expect(res.ok(), 'token valide accepté').toBeTruthy();
    await api.dispose();
  });

  test('token à signature falsifiée rejeté', async ({ playwright, users }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.get('/api/candidatures', {
      headers: { Authorization: `Bearer ${tamperSignature(users.a.accessToken)}` },
    });
    expect(res.status(), 'signature invalide → 401').toBe(401);
    await api.dispose();
  });

  test('token alg:none rejeté', async ({ playwright, users }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.get('/api/candidatures', {
      headers: { Authorization: `Bearer ${forgeAlgNone(users.a.accessToken)}` },
    });
    expect(res.status(), 'alg:none → 401').toBe(401);
    await api.dispose();
  });

  test('token bidon rejeté', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.get('/api/candidatures', {
      headers: { Authorization: 'Bearer aaa.bbb.ccc' },
    });
    expect(res.status(), 'token malformé → 401').toBe(401);
    await api.dispose();
  });

  test('escalade de rôle par payload falsifié impossible', async ({ playwright, users }) => {
    // On modifie le claim role → Admin, mais la signature devient invalide → 401 attendu.
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.get('/api/admin/users', {
      headers: { Authorization: `Bearer ${forgeAdminRole(users.a.accessToken)}` },
    });
    expect(res.status(), 'rôle forgé → toujours rejeté (401)').toBe(401);
    await api.dispose();
  });
});
