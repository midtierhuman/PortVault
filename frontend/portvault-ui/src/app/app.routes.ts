import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard';
import { AuthComponent } from './features/auth/auth';
import { authGuard, publicGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

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
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./features/admin/admin-dashboard/admin-dashboard').then(
        (m) => m.AdminDashboardComponent
      ),
  },
  { path: '**', redirectTo: '' },
];
