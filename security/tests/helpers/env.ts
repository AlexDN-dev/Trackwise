import os from 'node:os';
import path from 'node:path';

// Base de l'API locale. Override possible : TRACKWISE_API=https://localhost:7057
export const API_BASE = process.env.TRACKWISE_API ?? 'https://localhost:7057';

// Fichier où le DevFileEmailSender du back écrit les tokens capturés.
// DOIT correspondre à Email:DevCaptureFile côté back (défaut : os tmp + même nom de fichier).
// Si les deux process ne voient pas le même dossier tmp, fixe la même valeur des deux côtés :
//   back  : "Email:DevCaptureFile" = "/chemin/absolu/trackwise-dev-emails.jsonl"
//   suite : TRACKWISE_DEV_EMAILS=/chemin/absolu/trackwise-dev-emails.jsonl
export const DEV_EMAILS_FILE =
  process.env.TRACKWISE_DEV_EMAILS ?? path.join(os.tmpdir(), 'trackwise-dev-emails.jsonl');

// Mot de passe conforme à la politique (8+, 1 majuscule, 1 symbole).
export const VALID_PASSWORD = 'Pentest123!';
