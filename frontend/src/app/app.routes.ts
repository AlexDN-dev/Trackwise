import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth', pathMatch: 'full' },
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth').then((m) => m.Auth),
  },
  {
    path: 'candidatures',
    loadComponent: () => import('./features/candidatures/kanban').then((m) => m.Kanban),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'auth' },
];
