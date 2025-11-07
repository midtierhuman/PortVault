import { Routes } from '@angular/router';
import { PortfolioListComponent } from './portfolio-list/portfolio-list';
import { PortfolioDetailsComponent } from './portfolio-details/portfolio-details';

export const routes: Routes = [
  { path: '', component: PortfolioListComponent },
  { path: ':id', component: PortfolioDetailsComponent },
];
