// Le back limite les endpoints /api/auth à 5 requêtes / 30s par IP.
// Pour éviter des 429 parasites dans les tests fonctionnels, on throttle proactivement
// les appels auth côté suite (marge : 4/30s). Le spec dédié au rate-limit, lui, ne passe
// PAS par ce throttle (il veut justement déclencher le 429) et s'exécute en dernier.
const WINDOW_MS = 30_000;
const LIMIT = 4;
const timestamps: number[] = [];

export async function throttleAuth(): Promise<void> {
  for (;;) {
    const now = Date.now();
    while (timestamps.length && now - timestamps[0] >= WINDOW_MS) timestamps.shift();
    if (timestamps.length < LIMIT) {
      timestamps.push(now);
      return;
    }
    const waitMs = WINDOW_MS - (now - timestamps[0]) + 250;
    await new Promise((r) => setTimeout(r, waitMs));
  }
}
