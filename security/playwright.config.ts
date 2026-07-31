import { defineConfig } from '@playwright/test';
import { API_BASE } from './tests/helpers/env';

// Suite orientée sécurité : on teste surtout l'API (autorisation, tokens, rate-limit).
// Séquentiel (workers: 1) pour éviter que les tests de rate-limit s'interfèrent.
export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: 1,
  // Le back limite /api/auth à 5 req/30s. La suite throttle ses appels auth sur des
  // fenêtres de 30s → un test qui traverse une fenêtre a besoin de plus que 30s.
  timeout: 120_000,
  expect: { timeout: 10_000 },
  reporter: [['list']],
  use: {
    baseURL: API_BASE,
    ignoreHTTPSErrors: true, // cert self-signed en local (https://localhost:7057)
  },
});
