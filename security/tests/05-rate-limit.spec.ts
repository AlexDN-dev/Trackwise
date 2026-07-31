import { test, expect } from '@playwright/test';
import { API_BASE } from './helpers/env';
import { uniqueEmail } from './helpers/accounts';

/**
 * Vérifie que le rate-limiter "auth" (5 req / 30s par IP) protège bien les endpoints
 * sensibles contre le brute-force / spam. Ce spec s'exécute EN DERNIER car il consomme
 * volontairement la fenêtre de rate-limit (il n'utilise pas le throttle de la suite).
 *
 * Un échec ici = contrôle anti-abus absent ou trop permissif → finding.
 */
test.describe('Sécurité — rate limiting', () => {
  test('un burst sur forgot-password déclenche un 429', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });

    const statuses: number[] = [];
    for (let i = 0; i < 10; i++) {
      const res = await api.post('/api/auth/forgot-password', { data: { email: uniqueEmail() } });
      statuses.push(res.status());
    }

    expect(
      statuses.includes(429),
      `attendu au moins un 429 sur 10 requêtes rapides — obtenu : ${statuses.join(', ')}`,
    ).toBeTruthy();

    await api.dispose();
  });
});
