import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing').then((m) => m.Landing),
  },
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth').then((m) => m.Auth),
  },
  {
    path: 'candidatures',
    loadComponent: () => import('./features/candidatures/kanban').then((m) => m.Kanban),
    canActivate: [authGuard],
  },
  {
    path: 'statistiques',
    loadComponent: () => import('./features/stats/stats').then((m) => m.Stats),
    canActivate: [authGuard],
  },
  {
    path: 'profil',
    loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'informations' },
      {
        path: 'informations',
        loadComponent: () =>
          import('./features/profile/sections/informations/informations').then(
            (m) => m.ProfileInformations,
          ),
      },
      {
        path: 'securite',
        loadComponent: () =>
          import('./features/profile/sections/security/security').then(
            (m) => m.ProfileSecurity,
          ),
      },
      {
        path: 'competences',
        loadComponent: () =>
          import('./features/profile/sections/competences/competences').then(
            (m) => m.ProfileCompetences,
          ),
      },
      {
        path: 'abonnement',
        loadComponent: () =>
          import('./features/profile/sections/subscription/subscription').then(
            (m) => m.ProfileSubscription,
          ),
      },
    ],
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/admin').then((m) => m.Admin),
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'stats' },
      {
        path: 'stats',
        loadComponent: () =>
          import('./features/admin/sections/stats/admin-stats').then((m) => m.AdminStats),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/admin/sections/users/admin-users').then((m) => m.AdminUsers),
      },
      {
        path: 'ips',
        loadComponent: () =>
          import('./features/admin/sections/ips/admin-ips').then((m) => m.AdminIps),
      },
      {
        path: 'audit',
        loadComponent: () =>
          import('./features/admin/sections/audit/admin-audit').then((m) => m.AdminAudit),
      },
      {
        path: 'logs',
        loadComponent: () =>
          import('./features/admin/sections/logs/admin-logs').then((m) => m.AdminLogs),
      },
    ],
  },
  {
    path: 'guide',
    loadComponent: () => import('./features/pages/guide').then((m) => m.Guide),
  },
  {
    path: 'aide',
    loadComponent: () => import('./features/pages/aide').then((m) => m.Aide),
  },
  {
    path: 'confidentialite',
    loadComponent: () => import('./features/pages/confidentialite').then((m) => m.Confidentialite),
  },
  {
    path: 'conditions',
    loadComponent: () => import('./features/pages/conditions').then((m) => m.Conditions),
  },
  {
    path: 'cookies',
    loadComponent: () => import('./features/pages/cookies').then((m) => m.Cookies),
  },
  { path: '**', redirectTo: '' },
];
