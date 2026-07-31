import fs from 'node:fs';
import { DEV_EMAILS_FILE } from './env';

export interface DevEmail {
  to: string;
  type: 'confirmation' | 'reset' | 'password-changed';
  token: string | null;
  sentAt: string;
}

function readAll(): DevEmail[] {
  if (!fs.existsSync(DEV_EMAILS_FILE)) return [];
  return fs
    .readFileSync(DEV_EMAILS_FILE, 'utf8')
    .split('\n')
    .filter(Boolean)
    .map((line) => JSON.parse(line) as DevEmail);
}

/**
 * Attend qu'un email d'un type donné soit capturé pour une adresse, et renvoie son token.
 * `since` permet d'ignorer les emails d'un run précédent (on ne prend que ceux émis après).
 */
export async function waitForToken(
  to: string,
  type: DevEmail['type'],
  since: Date,
  timeoutMs = 5000,
): Promise<string> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const match = readAll()
      .filter(
        (e) =>
          e.to.toLowerCase() === to.toLowerCase() &&
          e.type === type &&
          new Date(e.sentAt) >= since &&
          !!e.token,
      )
      .pop();
    if (match?.token) return match.token;
    await new Promise((r) => setTimeout(r, 150));
  }
  throw new Error(
    `Aucun email "${type}" capturé pour ${to}.\n` +
      `Fichier attendu : ${DEV_EMAILS_FILE}\n` +
      `Vérifie que le back tourne avec Email:Provider=Dev et que les chemins concordent.`,
  );
}

/** Indique si un email d'un type donné a été émis pour une adresse après `since`. */
export function wasEmailSent(to: string, type: DevEmail['type'], since: Date): boolean {
  return readAll().some(
    (e) => e.to.toLowerCase() === to.toLowerCase() && e.type === type && new Date(e.sentAt) >= since,
  );
}
