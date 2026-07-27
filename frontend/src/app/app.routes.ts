import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

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
  { path: '**', redirectTo: '' },
];
