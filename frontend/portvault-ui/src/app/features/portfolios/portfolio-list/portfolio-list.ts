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
import {
  NgApexchartsModule,
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexStroke,
  ApexDataLabels,
  ApexTooltip,
  ApexPlotOptions,
  ApexYAxis,
} from 'ng-apexcharts';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Portfolio } from '../../../models/portfolio.model';
import { Holding } from '../../../models/holding.model';
import { Transaction, TradeType } from '../../../models/transaction.model';
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
    NgApexchartsModule,
  ],
  templateUrl: './portfolio-list.html',
  styleUrls: ['./portfolio-list.scss'],
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
  granularity = signal<'daily' | 'monthly' | 'yearly'>('monthly');
  chartSeries = signal<ApexAxisChartSeries>([]);
  chartOptions = signal<{
    chart: ApexChart;
    xaxis: ApexXAxis;
    yaxis: ApexYAxis | ApexYAxis[];
    stroke: ApexStroke;
    dataLabels: ApexDataLabels;
    tooltip: ApexTooltip;
    plotOptions: ApexPlotOptions;
  }>({
    chart: { type: 'line', height: 360, toolbar: { show: false } },
    xaxis: { categories: [] },
    dataLabels: { enabled: false },
    stroke: { width: [0, 3], curve: 'smooth' },
    plotOptions: { bar: { columnWidth: '60%' } },
    tooltip: { shared: true },
    yaxis: { labels: { formatter: (val) => `₹${Number(val).toLocaleString('en-IN')}` } },
  });

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
        this.buildChart();
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

  setGranularity(period: 'daily' | 'monthly' | 'yearly') {
    this.granularity.set(period);
    this.buildChart();
  }

  private buildChart() {
    const tx = this.transactions();
    if (!tx.length) {
      this.chartSeries.set([]);
      this.chartOptions.update((o) => ({ ...o, xaxis: { categories: [] } }));
      return;
    }

    const granularity = this.granularity();
    type Bucket = { key: string; date: Date; flow: number };
    const buckets = new Map<string, Bucket>();

    const keyFor = (date: Date) => {
      const y = date.getFullYear();
      const m = date.getMonth() + 1;
      const d = date.getDate();
      if (granularity === 'yearly') return `${y}`;
      if (granularity === 'monthly') return `${y}-${m.toString().padStart(2, '0')}`;
      return `${y}-${m.toString().padStart(2, '0')}-${d.toString().padStart(2, '0')}`;
    };

    tx.forEach((t) => {
      const date = new Date(t.tradeDate);
      if (Number.isNaN(date.getTime())) return;
      const amount = t.price * t.quantity * (t.tradeType === TradeType.Sell ? -1 : 1);
      const key = keyFor(date);
      const existing = buckets.get(key);
      if (existing) {
        existing.flow += amount;
        existing.date = date < existing.date ? date : existing.date;
      } else {
        buckets.set(key, { key, date, flow: amount });
      }
    });

    const sorted = Array.from(buckets.values()).sort((a, b) => a.date.getTime() - b.date.getTime());

    let cumulative = 0;
    const categories = sorted.map((b) => b.key);
    const netFlows: number[] = [];
    const cumulativeFlows: number[] = [];

    sorted.forEach((b) => {
      netFlows.push(b.flow);
      cumulative += b.flow;
      cumulativeFlows.push(cumulative);
    });

    this.chartSeries.set([
      { name: 'Net Flow', type: 'column', data: netFlows },
      { name: 'Cumulative Flow', type: 'line', data: cumulativeFlows },
    ]);

    this.chartOptions.set({
      chart: { type: 'line', height: 360, toolbar: { show: false } },
      xaxis: { categories, labels: { rotate: -45 } },
      dataLabels: { enabled: false },
      stroke: { width: [0, 3], curve: 'smooth' },
      plotOptions: { bar: { columnWidth: '60%' } },
      yaxis: {
        labels: {
          formatter: (val) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`,
        },
      },
      tooltip: {
        shared: true,
        y: {
          formatter: (val) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 2 })}`,
        },
      },
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
    this.chartSeries.set([]);
  }
}
