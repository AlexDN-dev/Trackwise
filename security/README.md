# Trackwise — Suite de pentest applicatif (Playwright)

Tests d'autorisation et de sécurité contre l'API **locale** (jamais la prod).
On teste surtout l'API directement (`APIRequestContext`) : c'est là que vit la logique
d'autorisation, de tokens et de rate-limit.

## Prérequis

1. **Back en mode dev** avec le sender d'email de capture activé :
   - `DevFileEmailSender` enregistré quand `Email:Provider = "Dev"`
   - dans `appsettings.Development.json` : `"Email": { "Provider": "Dev" }`
2. Back lancé en local (par défaut `https://localhost:7057`).
3. Base SQL locale accessible (les tests créent des comptes jetables).

## Installation

```bash
cd security
npm install
npx playwright install chromium
```

## Lancer les tests

```bash
cd security
npm test
```

## Configuration (variables d'env optionnelles)

| Variable | Défaut | Rôle |
|---|---|---|
| `TRACKWISE_API` | `https://localhost:7057` | Base URL de l'API à tester |
| `TRACKWISE_DEV_EMAILS` | `<tmp>/trackwise-dev-emails.jsonl` | Fichier où le back capture les tokens |

> ⚠️ Si un test échoue avec « Aucun email capturé », c'est que le back et la suite
> ne pointent pas le même fichier. Fixe la même valeur absolue des deux côtés :
> `Email:DevCaptureFile` (back) et `TRACKWISE_DEV_EMAILS` (suite).

## Couverture

| Fichier | Ce qui est testé |
|---|---|
| `01-email-tokens.spec.ts` | Tokens confirmation/reset à usage unique, token invalide → 400, anti-énumération |
| `02-jwt-auth.spec.ts` | Sans token → 401, signature falsifiée / `alg:none` / token bidon rejetés, escalade de rôle par payload forgé impossible |
| `03-access-control.spec.ts` | IDOR read/update/delete (user A ↔ B → 404), `/admin` non-admin → 403, anonyme → 401 |
| `04-stripe-webhook.spec.ts` | Webhook sans signature / signature invalide → 400 |
| `05-rate-limit.spec.ts` | Burst sur `forgot-password` → au moins un 429 (exécuté en dernier) |

### Non couvert automatiquement (par choix)

- **XSS** : Angular échappe par défaut le binding `{{ }}`, et la CSP est en `script-src 'self'`.
  Un stored-XSS via un champ candidature ne s'exécuterait pas au rendu → vérification manuelle ponctuelle suffisante.
- **Token expiré** : l'access token vit 15 min → non rejouable rapidement en suite. La rotation/révocation
  du refresh token est le vrai point sensible (à ajouter si besoin).
- **Headers de sécurité de l'API** : déjà posés par `SecurityHeadersMiddleware` côté back, et par nginx côté front.

## Ordre & rate-limit

Les endpoints `/api/auth/*` sont limités à **5 req / 30s par IP**. La suite throttle
proactivement ses appels auth (`helpers/authThrottle.ts`) pour ne pas fausser les tests
fonctionnels ; seul `05-rate-limit` consomme volontairement la fenêtre, d'où son exécution
en dernier. Un run complet prend donc ~1-2 min (fenêtres de rate-limit).
