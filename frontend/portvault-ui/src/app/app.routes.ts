import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  {
    path: 'portfolios',
    loadChildren: () => import('./features/portfolios/portfolios.routes').then((m) => m.routes),
  },
];
