import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard';
import { AuthComponent } from './features/auth/auth';
import { authGuard, publicGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthComponent,
    canActivate: [publicGuard],
  },
  {
    path: '',
    component: DashboardComponent,
    canActivate: [authGuard],
  },
  {
    path: 'portfolios',
    canActivate: [authGuard],
    loadChildren: () => import('./features/portfolios/portfolios.routes').then((m) => m.routes),
  },
  { path: '**', redirectTo: '' },
];
