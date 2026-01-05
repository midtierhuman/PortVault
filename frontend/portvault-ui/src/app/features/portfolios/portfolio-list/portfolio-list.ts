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
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
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
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
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
  showDetails = signal(false);
  page = signal(1);
  pageSize = signal(20);
  totalPages = signal(0);
  totalCount = signal(0);
  listFilterFrom = signal<Date | null>(this.getDefaultFromDate());
  listFilterTo = signal<Date | null>(this.getDefaultToDate());
  listFilterSearch = signal<string>('');
  sortField = signal<
    'symbol' | 'tradeType' | 'quantity' | 'price' | 'tradeDate' | 'segment' | 'tradeID' | null
  >(null);
  sortDir = signal<'asc' | 'desc'>('asc');
  isLoading = signal(true);
  isLoadingDetails = signal(false);
  isLoadingTransactions = signal(false);
  isLoadingChart = signal(false);
  duration = signal<'1M' | '3M' | '6M' | 'YTD' | '1Y' | '3Y' | '5Y' | 'ALL'>('ALL');
  frequency = signal<'Daily' | 'Weekly' | 'Monthly'>('Daily');
  analyticsHistory = signal<{ date: string; invested: number }[]>([]);
  segmentAllocation = signal<{ segment: string; value: number; percentage: number }[]>([]);
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

  private getDefaultFromDate(): Date {
    const date = new Date();
    date.setFullYear(date.getFullYear() - 1);
    return date;
  }

  private getDefaultToDate(): Date {
    return new Date();
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
    this.page.set(1);
    this.showDetails.set(false);
    this.loadPortfolioDetails(portfolio.name);
  }

  private async loadPortfolioDetails(portfolioName: string) {
    this.isLoadingDetails.set(true);
    try {
      const [holdings] = await Promise.all([
        this.portfolioService.getHoldings(portfolioName),
        this.loadTransactionsForChart(false),
      ]);
      this.holdings.set(holdings);
    } catch (error) {
      console.error('Failed to load portfolio details:', error);
      this.snackBar.open('Failed to load portfolio details. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    } finally {
      this.isLoadingDetails.set(false);
    }
  }

  private async loadTransactions(page = this.page(), showSpinner = true) {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;
    if (showSpinner) this.isLoadingTransactions.set(true);
    try {
      const fromDate = this.listFilterFrom();
      const toDate = this.listFilterTo();
      const res = await this.portfolioService.getTransactions(portfolioName, {
        page,
        pageSize: this.pageSize(),
        from: fromDate ? fromDate.toISOString().split('T')[0] : undefined,
        to: toDate ? toDate.toISOString().split('T')[0] : undefined,
        search: this.listFilterSearch()?.trim() || undefined,
      });
      this.transactions.set(this.sortData(res.data));
      this.page.set(res.page);
      this.pageSize.set(res.pageSize);
      this.totalPages.set(res.totalPages);
      this.totalCount.set(res.totalCount);
    } catch (error) {
      console.error('Failed to load transactions:', error);
      this.snackBar.open('Failed to load transactions. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    } finally {
      if (showSpinner) this.isLoadingTransactions.set(false);
    }
  }

  private async loadTransactionsForChart(showSpinner = true) {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;
    if (showSpinner) this.isLoadingChart.set(true);
    try {
      const analytics = await this.portfolioService.getAnalytics(
        portfolioName,
        this.duration(),
        this.frequency()
      );
      this.analyticsHistory.set(analytics.history);
      this.segmentAllocation.set(analytics.segmentAllocation);
      this.buildChart();
    } catch (error) {
      console.error('Failed to load analytics:', error);
      this.snackBar.open('Failed to load analytics. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    } finally {
      if (showSpinner) this.isLoadingChart.set(false);
    }
  }

  applyListFilters() {
    if (!this.selectedPortfolio()) return;
    this.page.set(1);
    this.loadTransactions(1);
  }

  resetListFilters() {
    if (!this.selectedPortfolio()) return;
    this.listFilterFrom.set(this.getDefaultFromDate());
    this.listFilterTo.set(this.getDefaultToDate());
    this.listFilterSearch.set('');
    this.applyListFilters();
  }

  applyChartFilters() {
    if (!this.selectedPortfolio()) return;
    this.loadTransactionsForChart();
  }

  resetChartFilters() {
    if (!this.selectedPortfolio()) return;
    this.duration.set('ALL');
    this.frequency.set('Daily');
    this.applyChartFilters();
  }

  changeDuration(value: '1M' | '3M' | '6M' | 'YTD' | '1Y' | '3Y' | '5Y' | 'ALL') {
    this.duration.set(value);
    this.loadTransactionsForChart();
  }

  changeFrequency(value: 'Daily' | 'Weekly' | 'Monthly') {
    this.frequency.set(value);
    this.loadTransactionsForChart();
  }

  changePage(delta: number) {
    if (!this.selectedPortfolio()) return;
    const target = this.page() + delta;
    if (target < 1 || (this.totalPages() && target > this.totalPages())) return;
    this.loadTransactions(target);
  }

  changePageSize(size: number) {
    if (!this.selectedPortfolio()) return;
    this.pageSize.set(size);
    this.page.set(1);
    this.loadTransactions(1);
  }

  toggleDetails() {
    if (!this.selectedPortfolio()) return;
    const next = !this.showDetails();
    this.showDetails.set(next);
    if (next && this.transactions().length === 0) {
      this.loadTransactions(1);
    }
  }

  sortBy(
    field: 'symbol' | 'tradeType' | 'quantity' | 'price' | 'tradeDate' | 'segment' | 'tradeID'
  ) {
    const currentField = this.sortField();
    const currentDir = this.sortDir();
    const nextDir = currentField === field && currentDir === 'asc' ? 'desc' : 'asc';
    this.sortField.set(field);
    this.sortDir.set(nextDir);
    this.transactions.set(this.sortData(this.transactions()));
  }

  private sortData(data: Transaction[]) {
    const field = this.sortField();
    const dir = this.sortDir();
    if (!field) return data;
    const sorted = [...data].sort((a, b) => {
      const va: any = a[field];
      const vb: any = b[field];
      if (field === 'tradeDate') {
        const da = new Date(va).getTime();
        const db = new Date(vb).getTime();
        return da - db;
      }
      if (typeof va === 'number' && typeof vb === 'number') return va - vb;
      return String(va ?? '').localeCompare(String(vb ?? ''));
    });
    return dir === 'asc' ? sorted : sorted.reverse();
  }

  private buildChart() {
    const history = this.analyticsHistory();
    if (!history.length) {
      this.chartSeries.set([]);
      this.chartOptions.update((o) => ({ ...o, xaxis: { categories: [] } }));
      return;
    }

    const categories = history.map((h) => h.date);
    const investedValues = history.map((h) => h.invested);

    this.chartSeries.set([{ name: 'Invested Amount', type: 'area', data: investedValues }]);

    this.chartOptions.set({
      chart: { type: 'area', height: 360, toolbar: { show: false } },
      xaxis: { categories, labels: { rotate: -45 } },
      dataLabels: { enabled: false },
      stroke: { width: 2, curve: 'smooth' },
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
      plotOptions: {},
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
    this.analyticsHistory.set([]);
    this.segmentAllocation.set([]);
    this.totalPages.set(0);
    this.totalCount.set(0);
    this.page.set(1);
    this.showDetails.set(false);
  }
}
