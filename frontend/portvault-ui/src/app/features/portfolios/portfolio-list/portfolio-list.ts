import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { PortfolioService } from '../../../core/services/portfolio.service';

@Component({
  selector: 'app-portfolio-list',
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule],
  templateUrl: './portfolio-list.html',
  styleUrl: './portfolio-list.scss',
})
export class PortfolioListComponent {
  constructor(private router: Router, private portfolioService: PortfolioService) {}

  portfolios = signal<Portfolio[]>([]);

  ngOnInit() {
    this.loadPortfolios();
  }

  private loadPortfolios() {
    this.portfolioService.getAll().then((p) => {
      this.portfolios.set(p);
    });
  }

  open(id: string) {
    this.router.navigate(['/portfolios', id]);
  }
}
