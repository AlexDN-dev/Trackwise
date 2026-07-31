import { test, expect } from '@playwright/test';
import { API_BASE, VALID_PASSWORD } from './helpers/env';
import { uniqueEmail } from './helpers/accounts';
import { waitForToken } from './helpers/devEmails';
import { postAuth } from './helpers/http';

/**
 * Sécurité des tokens email (confirmation + reset).
 * Valide aussi le harness : si ces tests passent, la capture des tokens fonctionne.
 */
test.describe('Sécurité — tokens email', () => {
  test('token de confirmation à usage unique', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const email = uniqueEmail();
    const since = new Date();

    const reg = await postAuth(api, '/api/auth/register', {
      firstname: 'Pen',
      lastname: 'Test',
      email,
      password: VALID_PASSWORD,
    });
    expect(reg.ok(), 'inscription acceptée').toBeTruthy();

    const token = await waitForToken(email, 'confirmation', since);

    const first = await postAuth(api, '/api/auth/confirm-email', { token });
    expect(first.status(), 'première confirmation acceptée').toBe(200);

    const second = await postAuth(api, '/api/auth/confirm-email', { token });
    expect(second.ok(), 'un token déjà consommé ne doit PAS être réutilisable').toBeFalsy();

    await api.dispose();
  });

  test('token de confirmation invalide rejeté', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });

    const res = await postAuth(api, '/api/auth/confirm-email', {
      token: 'ceci-nest-pas-un-token-valide',
    });
    expect(res.status(), 'un token bidon doit être rejeté en 400').toBe(400);

    await api.dispose();
  });

  test('token de reset à usage unique', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const email = uniqueEmail();

    let since = new Date();
    await postAuth(api, '/api/auth/register', {
      firstname: 'Pen',
      lastname: 'Test',
      email,
      password: VALID_PASSWORD,
    });
    const confToken = await waitForToken(email, 'confirmation', since);
    await postAuth(api, '/api/auth/confirm-email', { token: confToken });

    since = new Date();
    const forgot = await postAuth(api, '/api/auth/forgot-password', { email });
    expect(forgot.ok(), 'forgot-password répond 200 (générique)').toBeTruthy();
    const resetToken = await waitForToken(email, 'reset', since);

    const first = await postAuth(api, '/api/auth/reset-password', {
      token: resetToken,
      newPassword: 'Reset123!',
    });
    expect(first.ok(), 'premier reset accepté').toBeTruthy();

    const second = await postAuth(api, '/api/auth/reset-password', {
      token: resetToken,
      newPassword: 'Autre123!',
    });
    expect(second.ok(), 'un token de reset déjà consommé ne doit PAS être réutilisable').toBeFalsy();

    await api.dispose();
  });

  test('forgot-password ne divulgue pas l’existence du compte (anti-énumération)', async ({
    playwright,
  }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });

    const unknown = await postAuth(api, '/api/auth/forgot-password', {
      email: uniqueEmail('inconnu'),
    });
    expect(unknown.status(), 'réponse 200 identique pour un email inconnu').toBe(200);

    await api.dispose();
  });
});
