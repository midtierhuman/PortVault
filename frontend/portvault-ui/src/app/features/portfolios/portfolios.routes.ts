import { Routes } from '@angular/router';
import { PortfolioList } from './portfolio-list/portfolio-list';
import { PortfolioDetails } from './portfolio-details/portfolio-details';

export const routes: Routes = [
  { path: '', component: PortfolioList },
  { path: ':id', component: PortfolioDetails },
];
