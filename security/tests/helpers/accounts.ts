import { APIRequestContext } from '@playwright/test';
import { VALID_PASSWORD } from './env';
import { waitForToken } from './devEmails';
import { postAuth } from './http';

let counter = 0;

/** Adresse jetable unique (évite les collisions entre runs). */
export function uniqueEmail(prefix = 'pentest'): string {
  counter += 1;
  return `${prefix}+${Date.now()}-${counter}@example.com`;
}

export interface TestAccount {
  email: string;
  password: string;
  accessToken: string;
}

/**
 * Inscrit un compte, récupère le token de confirmation capturé, confirme l'email,
 * et renvoie l'access token (auto-login via /confirm-email).
 * Le contexte `api` conserve le cookie refresh (HttpOnly) pour ce compte.
 */
export async function registerAndConfirm(
  api: APIRequestContext,
  email = uniqueEmail(),
  password = VALID_PASSWORD,
): Promise<TestAccount> {
  const since = new Date();

  const reg = await postAuth(api, '/api/auth/register', {
    firstname: 'Pen',
    lastname: 'Test',
    email,
    password,
  });
  if (!reg.ok()) {
    throw new Error(`register a échoué (${reg.status()}): ${await reg.text()}`);
  }

  const token = await waitForToken(email, 'confirmation', since);

  const confirm = await postAuth(api, '/api/auth/confirm-email', { token });
  if (!confirm.ok()) {
    throw new Error(`confirm-email a échoué (${confirm.status()}): ${await confirm.text()}`);
  }

  const { accessToken } = (await confirm.json()) as { accessToken: string };
  return { email, password, accessToken };
}
