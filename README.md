# Trackwise

Application web de suivi de candidatures avec analyse IA d'offres d'emploi, score de compatibilité, statistiques et abonnement Premium.

Projet de fin de formation. Deadline : 3 août 2026.

## Stack

**Backend**
- .NET 10 (ASP.NET Core, Controllers)
- Entity Framework Core 10 + SQL Server (Docker)
- Clean Architecture (Domain / Application / Infrastructure / API)
- JWT + refresh tokens en cookies HttpOnly, BCrypt pour les mots de passe
- Serilog (console + rolling files) avec enrichisseur de redaction (emails hashés SHA-256, JWT strippés)
- Stripe.net pour la facturation Premium (Checkout + Customer Portal + Webhook)
- Provider IA interchangeable : Ollama local (dev) ou Azure OpenAI (prod)

**Frontend**
- Angular 22 (standalone components, signals, zoneless)
- PrimeNG 22 (composants + PrimeIcons)
- Angular CDK Drag & Drop pour le kanban
- Design system maison (indigo `#5b53e8` / Plus Jakarta Sans)
- Access token en mémoire uniquement, refresh via cookie HttpOnly

**Infrastructure**
- SQL Server 2022 (conteneur Docker)
- Azure OpenAI Service (gpt-5-mini en région Germany West Central)
- Déploiement cible : Dokploy sur VPS Hostinger (Ubuntu 24.04)

## Fonctionnalités principales

- Kanban de candidatures : drag & drop entre statuts, recherche instantanée, filtres
- Analyse IA d'une offre d'emploi (extraction titre, entreprise, localisation, contrat, compétences)
- Score de compatibilité candidat/offre (Jaccard directionnel)
- Statistiques : tableau de bord Free et Premium (avec floutage sur les sections Premium pour les Free)
- Profil utilisateur : informations, sécurité (changement de mot de passe), compétences, abonnement
- Abonnement Premium via Stripe (Checkout, Portal, webhook signé)
- Quotas d'utilisation IA par plan : 10 analyses/mois pour Free, 500 pour Premium (glissant sur 30 jours)
- Panel administrateur : gestion des utilisateurs (ban, unban, trial Premium, tokens IA bonus), ban d'IP, audit trail, viewer de logs, statistiques globales

## Sécurité

Le projet a fait l'objet d'un audit sécurité couvrant :

- Anti-énumération sur `/auth/register` (202 Accepted uniforme)
- Single-flight refresh (protection contre la révocation en cascade des refresh tokens)
- Refresh token en cookie `HttpOnly + Secure + SameSite=Strict`, access token en mémoire uniquement
- Politique de mot de passe (8 caractères min, 1 majuscule, 1 symbole)
- CORS avec whitelist d'origines, `AllowedHosts` restreint
- Headers de sécurité (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`, `Cross-Origin-Opener-Policy`)
- CSP strict (pas de `unsafe-eval`, `default-src 'self'`)
- Endpoint de changement de mot de passe avec révocation immédiate de toutes les sessions
- Rate limiting global (100/10s) + auth (5/30s) + IA (5/min)
- Ban par IP avec middleware en tête de pipeline
- Toutes les actions administrateur tracées en base et via Serilog

## Prérequis

- .NET SDK 10 (`~/.dotnet` en local)
- Node.js 20+
- Docker Desktop
- Azure CLI (uniquement pour le provisionnement Azure OpenAI)
- Stripe CLI (uniquement pour tester les webhooks en local)

## Installation

### 1. Cloner et restaurer

```bash
git clone <repo-url> trackwise
cd trackwise

# Backend
cd backend
dotnet restore

# Frontend
cd ../frontend
npm install
```

### 2. SQL Server dans Docker

```bash
docker run -d \
  --name trackwise-sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=<mot-de-passe-fort>" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### 3. User Secrets (jamais commit)

Depuis `backend/src/Trackwise.API/` :

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=TrackwiseDb;User Id=sa;Password=<mot-de-passe>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<chaine-aleatoire-256-bits>"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
dotnet user-secrets set "Stripe:PriceId" "price_..."
```

Pour l'IA en production (Azure) :

```bash
dotnet user-secrets set "AzureOpenAi:Endpoint" "https://<resource>.openai.azure.com/"
dotnet user-secrets set "AzureOpenAi:Deployment" "gpt-5-mini"
dotnet user-secrets set "AzureOpenAi:ApiKey" "<cle-api>"
```

### 4. Migrations EF Core

Depuis `backend/src/Trackwise.API/` :

```bash
dotnet ef database update --project ../Trackwise.Infrastructure --startup-project .
```

## Configuration du provider IA

Dans `backend/src/Trackwise.API/appsettings.json` :

```json
"Ai": {
  "Provider": "Ollama"
}
```

Valeurs acceptées :
- `Ollama` (par défaut) : requiert un Ollama local sur `http://localhost:11434` avec le modèle `qwen2.5:7b-instruct-q4_K_M`
- `AzureOpenAi` : nécessite les user-secrets `AzureOpenAi:*`

Provisionnement Azure OpenAI :

```bash
./backend/scripts/setup-azure-openai.sh
```

## Lancement

### Backend

```bash
cd backend/src/Trackwise.API
dotnet run --launch-profile https
```

L'API est exposée sur :
- `https://localhost:7057`
- `http://localhost:5238`

Documentation OpenAPI (dev uniquement) : `https://localhost:7057/scalar`

### Frontend

```bash
cd frontend
npm run start
```

Accessible sur `http://localhost:4200`.

### Stripe webhook local

```bash
stripe listen --forward-to https://localhost:7057/api/stripe/webhook --skip-verify
```

## Création d'un administrateur

Il n'existe volontairement aucun endpoint API pour promouvoir un utilisateur en administrateur : le rôle se change uniquement en base, manuellement.

```bash
# Éditer l'email dans backend/scripts/promote-admin.sql, puis :
docker exec -i trackwise-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<mot-de-passe>' -C -d TrackwiseDb -I \
  < backend/scripts/promote-admin.sql
```

Après promotion, l'utilisateur doit se reconnecter pour que son JWT contienne le nouveau claim `role: Admin`.

## Architecture

```
Trackwise.Domain          Entités et enums, aucune dépendance
Trackwise.Application     Services, interfaces, DTOs, validation
Trackwise.Infrastructure  EF Core, repositories, Stripe, IA, BCrypt, TokenService
Trackwise.API             Controllers, middleware, DI, Program.cs
```

Le frontend suit une organisation par feature :

```
src/app/core         services, guards, interceptors, models
src/app/shared       composants réutilisables (toast, quota badge, confirm dialog)
src/app/features     auth, candidatures, profile, stats, admin, landing
```

## Logs

Les fichiers Serilog sont écrits dans `backend/src/Trackwise.API/logs/trackwise-YYYYMMDD.log`.

Rétention : 14 jours, roll quotidien ou tous les 10 Mo.

Format :

```
2026-07-28 14:23:01.123 +02:00 [INF] Trackwise.API.Program Provider IA actif : Ollama
```

Les emails apparaissant dans les logs sont automatiquement hashés (`email:sha256:xxxxxxxx`) et les JWT sont remplacés par `[REDACTED_JWT]`.

Un viewer intégré au panel administrateur permet de filtrer par niveau et par recherche texte.

## Structure des scripts

`backend/scripts/`
- `setup-azure-openai.sh` : provisionne le resource group, le compte Azure OpenAI et le déploiement du modèle
- `check-azure-openai-models.sh` : liste les modèles disponibles dans chaque région autorisée de la souscription
- `promote-admin.sql` : promeut un utilisateur en administrateur (édition manuelle de l'email)

## Roadmap restante

- Déploiement Dokploy (backend + frontend + base de données)
- Configuration `appsettings.Production.json` (CORS domaines réels, `AllowedHosts`)
- SMTP pour flow de reset mot de passe
- Externalisation des logs sur un service dédié
