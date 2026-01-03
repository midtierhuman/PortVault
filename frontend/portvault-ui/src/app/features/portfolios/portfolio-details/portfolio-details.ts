import { Component, signal, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { TransactionDialogComponent } from './transaction-dialog/transaction-dialog';
import { Holding } from '../../../models/holding.model';

@Component({
  selector: 'app-portfolio-details',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './portfolio-details.html',
  styleUrl: './portfolio-details.scss',
  standalone: true,
})
export class PortfolioDetailsComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private portfolioService = inject(PortfolioService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  id: string | null = null;
  holdings = signal<Holding[] | null>(null);
  isLoading = signal(true);

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.fetchHoldings(this.id);
    } else {
      this.router.navigate(['../']);
    }
  }

  private fetchHoldings(id: string) {
    this.isLoading.set(true);
    this.portfolioService
      .getHoldings(id)
      .then((p) => {
        this.holdings.set(p);
      })
      .catch((error) => {
        console.error('Failed to load holdings:', error);
        this.snackBar.open('Failed to load holdings. Please try again.', 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.router.navigate(['/portfolios']);
      })
      .finally(() => {
        this.isLoading.set(false);
      });
  }

  openTransactions(h: Holding) {
    this.dialog.open(TransactionDialogComponent, { data: h });
  }
}
