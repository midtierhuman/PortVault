import { CommonModule } from '@angular/common';
import { Component, signal, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Portfolio } from '../../../models/portfolio.model';

@Component({
  selector: 'app-portfolio-list',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './portfolio-list.html',
  styleUrl: './portfolio-list.scss',
})
export class PortfolioListComponent {
  private router = inject(Router);
  private portfolioService = inject(PortfolioService);
  private snackBar = inject(MatSnackBar);

  portfolios = signal<Portfolio[]>([]);
  isLoading = signal(true);

  ngOnInit() {
    this.loadPortfolios();
  }

  private loadPortfolios() {
    this.isLoading.set(true);
    this.portfolioService
      .getAll()
      .then((p) => {
        this.portfolios.set(p);
      })
      .catch((error) => {
        console.error('Failed to load portfolios:', error);
        this.snackBar.open('Failed to load portfolios. Please try again.', 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      })
      .finally(() => {
        this.isLoading.set(false);
      });
  }

  open(id: string) {
    this.router.navigate(['/portfolios', id]);
  }
}
