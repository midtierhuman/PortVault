import { CommonModule } from '@angular/common';
import { Component, signal, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Portfolio } from '../../../models/portfolio.model';
import { Holding } from '../../../models/holding.model';
import { Transaction } from '../../../models/transaction.model';
import { TransactionEditDialogComponent } from './transaction-edit-dialog/transaction-edit-dialog';

@Component({
  selector: 'app-portfolio-list',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatIconModule,
    MatDialogModule,
  ],
  templateUrl: './portfolio-list.html',
  styleUrl: './portfolio-list.scss',
})
export class PortfolioListComponent {
  private portfolioService = inject(PortfolioService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);

  portfolios = signal<Portfolio[]>([]);
  selectedPortfolio = signal<Portfolio | null>(null);
  holdings = signal<Holding[]>([]);
  transactions = signal<Transaction[]>([]);
  isLoading = signal(true);
  isLoadingDetails = signal(false);

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

  selectPortfolio(portfolio: Portfolio) {
    this.selectedPortfolio.set(portfolio);
    this.loadPortfolioDetails(portfolio.name);
  }

  private loadPortfolioDetails(portfolioName: string) {
    this.isLoadingDetails.set(true);

    Promise.all([
      this.portfolioService.getHoldings(portfolioName),
      this.portfolioService.getTransactions(portfolioName),
    ])
      .then(([holdings, transactions]) => {
        this.holdings.set(holdings);
        this.transactions.set(transactions);
      })
      .catch((error) => {
        console.error('Failed to load portfolio details:', error);
        this.snackBar.open('Failed to load portfolio details. Please try again.', 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      })
      .finally(() => {
        this.isLoadingDetails.set(false);
      });
  }

  editTransaction(transaction: Transaction) {
    const dialogRef = this.dialog.open(TransactionEditDialogComponent, {
      width: '600px',
      data: { ...transaction },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && this.selectedPortfolio()) {
        this.updateTransaction(result);
      }
    });
  }

  private updateTransaction(transaction: Transaction) {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    this.portfolioService
      .updateTransaction(portfolioName, transaction)
      .then(() => {
        this.snackBar.open('Transaction updated successfully', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
        this.loadPortfolioDetails(portfolioName);
      })
      .catch((error) => {
        console.error('Failed to update transaction:', error);
        this.snackBar.open('Failed to update transaction. Please try again.', 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });
      });
  }

  backToList() {
    this.selectedPortfolio.set(null);
    this.holdings.set([]);
    this.transactions.set([]);
  }
}
