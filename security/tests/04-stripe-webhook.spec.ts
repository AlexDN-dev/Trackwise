import { test, expect } from '@playwright/test';
import { API_BASE } from './helpers/env';

/**
 * Le webhook Stripe est [AllowAnonymous] : sa seule protection est la vérification
 * de signature (Stripe-Signature). Une requête forgée doit être rejetée en 400,
 * sinon n'importe qui pourrait injecter de faux événements (ex. "abonnement payé").
 */
test.describe('Sécurité — webhook Stripe', () => {
  test('sans signature → 400', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.post('/api/stripe/webhook', {
      data: { type: 'checkout.session.completed', data: { object: {} } },
    });
    expect(res.status(), 'événement non signé rejeté').toBe(400);
    await api.dispose();
  });

  test('signature invalide → 400', async ({ playwright }) => {
    const api = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await api.post('/api/stripe/webhook', {
      headers: { 'Stripe-Signature': 't=1700000000,v1=deadbeefdeadbeefdeadbeef' },
      data: { type: 'checkout.session.completed', data: { object: {} } },
    });
    expect(res.status(), 'signature bidon rejetée').toBe(400);
    await api.dispose();
  });
});
