import { test as base, request, APIRequestContext } from '@playwright/test';
import { API_BASE } from './env';
import { registerAndConfirm, TestAccount } from './accounts';

// Deux utilisateurs confirmés, créés UNE seule fois par worker (workers: 1 → une fois au total)
// et réutilisés par les tests d'autorisation / IDOR / JWT. Chaque user garde son propre
// contexte API (donc son propre access token + cookie refresh).
interface Users {
  a: TestAccount;
  b: TestAccount;
  apiA: APIRequestContext;
  apiB: APIRequestContext;
}

export const test = base.extend<Record<string, never>, { users: Users }>({
  users: [
    async ({}, use) => {
      const apiA = await request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
      const apiB = await request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });

      const a = await registerAndConfirm(apiA);
      const b = await registerAndConfirm(apiB);

      await use({ a, b, apiA, apiB });

      await apiA.dispose();
      await apiB.dispose();
    },
    { scope: 'worker' },
  ],
});

export { expect } from '@playwright/test';
