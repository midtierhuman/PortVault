import { CommonModule } from '@angular/common';
import { Component, signal, inject, computed, linkedSignal, effect } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
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
import { firstValueFrom, of } from 'rxjs';
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
import {
  Transaction,
  TransactionPage,
  CreateTransactionRequest,
} from '../../../models/transaction.model';
import { TransactionEditDialogComponent } from './transaction-edit-dialog/transaction-edit-dialog';
import { TransactionAddDialogComponent } from './transaction-add-dialog/transaction-add-dialog';

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

  // -- Component State --
  selectedPortfolio = signal<Portfolio | null>(null);

  // Linked states that reset/update when portfolio changes
  page = linkedSignal({
    source: this.selectedPortfolio,
    computation: () => 1,
  });

  showDetails = linkedSignal({
    source: this.selectedPortfolio,
    computation: () => false,
  });

  // Filter State
  listFilterFrom = signal<Date | null>(this.getDefaultFromDate());
  listFilterTo = signal<Date | null>(this.getDefaultToDate());
  listFilterSearch = signal<string>('');

  pageSize = signal(20);

  sortField = signal<keyof Transaction | null>(null);
  sortDir = signal<'asc' | 'desc'>('asc');

  // Chart State
  duration = signal<'1M' | '3M' | '6M' | 'YTD' | '1Y' | '3Y' | '5Y' | 'ALL'>('ALL');
  frequency = signal<'Daily' | 'Weekly' | 'Monthly'>('Daily');

  // -- Resources (Data Fetching) --

  // 1. Portfolios
  portfoliosResource = rxResource({
    stream: () => this.portfolioService.getAll(),
  });

  // 2. Holdings (depends on selectedPortfolio)
  holdingsResource = rxResource({
    params: () => this.selectedPortfolio()?.name,
    stream: ({ params }) => {
      if (!params) return of([]);
      return this.portfolioService.getHoldings(params);
    },
  });

  // 3. Transactions (depends on portfolio, page, filters)
  transactionsResource = rxResource({
    params: () => {
      const p = this.selectedPortfolio();
      if (!p) return null;
      return {
        name: p.name,
        page: this.page(),
        pageSize: this.pageSize(),
        from: this.listFilterFrom(),
        to: this.listFilterTo(),
        search: this.listFilterSearch(), // Usually debouncing is handled by signal updates or manual trigger
      };
    },
    stream: ({ params }) => {
      if (!params) return of(this.getEmptyTransactionPage());
      return this.portfolioService.getTransactions(params.name, {
        page: params.page,
        pageSize: params.pageSize,
        from: params.from?.toISOString().split('T')[0],
        to: params.to?.toISOString().split('T')[0],
        search: params.search || undefined,
      });
    },
  });

  // 4. Analytics (Chart Data)
  analyticsResource = rxResource({
    params: () => {
      const p = this.selectedPortfolio();
      if (!p) return null;
      return { name: p.name, duration: this.duration(), frequency: this.frequency() };
    },
    stream: ({ params }) => {
      if (!params) return of({ history: [], segmentAllocation: [] });
      return this.portfolioService.getAnalytics(params.name, params.duration, params.frequency);
    },
  });

  // -- Computed Views --

  portfolios = computed(() => this.portfoliosResource.value() || []);
  holdings = computed(() => this.holdingsResource.value() || []);

  // Sorted Transactions
  transactions = computed(() => {
    const rawData = this.transactionsResource.value()?.data || [];
    return this.sortData(rawData);
  });

  totalPages = computed(() => this.transactionsResource.value()?.totalPages || 0);
  totalCount = computed(() => this.transactionsResource.value()?.totalCount || 0);

  // Analytics Chart Options
  investmentTrendChartOptions = computed(() => {
    const history = this.analyticsResource.value()?.history || [];
    const categories = history.map((h) => h.date);
    const investedValues = history.map((h) => h.invested);

    return {
      chart: { type: 'area', height: 350, toolbar: { show: false } } as ApexChart,
      xaxis: { categories, labels: { rotate: -45 } } as ApexXAxis,
      dataLabels: { enabled: false } as ApexDataLabels,
      stroke: { width: 2, curve: 'smooth' } as ApexStroke,
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.7,
          opacityTo: 0.3,
        },
      },
      yaxis: {
        labels: {
          formatter: (val: number) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`,
        },
      } as ApexYAxis,
      tooltip: {
        shared: true,
        y: {
          formatter: (val: number) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 2 })}`,
        },
      } as ApexTooltip,
      series: [
        { name: 'Invested Amount', type: 'area', data: investedValues },
      ] as ApexAxisChartSeries,
    };
  });

  annualInvestmentChartOptions = computed(() => {
    const history = this.analyticsResource.value()?.history || [];
    const annualData = this.aggregateByYear(history);

    return {
      chart: { type: 'bar', height: 350, toolbar: { show: false } } as ApexChart,
      xaxis: { categories: annualData.map((d) => d.year) } as ApexXAxis,
      dataLabels: { enabled: false } as ApexDataLabels,
      plotOptions: {
        bar: {
          borderRadius: 8,
          columnWidth: '60%',
        },
      } as ApexPlotOptions,
      yaxis: {
        labels: {
          formatter: (val: number) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`,
        },
      } as ApexYAxis,
      tooltip: {
        y: {
          formatter: (val: number) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 2 })}`,
        },
      } as ApexTooltip,
      series: [
        { name: 'Annual Investment', data: annualData.map((d) => d.invested) },
      ] as ApexAxisChartSeries,
    };
  });

  segmentAllocationChartOptions = computed(() => {
    const segmentAllocation = this.analyticsResource.value()?.segmentAllocation || [];

    return {
      chart: { type: 'donut', height: 350 } as ApexChart,
      labels: segmentAllocation.map((s) => s.segment),
      series: segmentAllocation.map((s) => s.value),
      dataLabels: {
        enabled: true,
        formatter: (val: number) => `${val.toFixed(1)}%`,
      } as ApexDataLabels,
      legend: {
        position: 'bottom' as const,
      },
      tooltip: {
        y: {
          formatter: (val: number) =>
            `₹${Number(val).toLocaleString('en-IN', { maximumFractionDigits: 2 })}`,
        },
      } as ApexTooltip,
      plotOptions: {
        pie: {
          donut: {
            size: '65%',
            labels: {
              show: true,
              total: {
                show: true,
                label: 'Total Value',
                formatter: (w: any) => {
                  const total = w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0);
                  return `₹${Number(total).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`;
                },
              },
            },
          },
        },
      } as ApexPlotOptions,
    };
  });

  // Portfolio Summary Metrics
  portfolioMetrics = computed(() => {
    const portfolio = this.selectedPortfolio();
    const holdings = this.holdings();

    if (!portfolio) return null;

    const totalInvested = portfolio.invested;
    const totalCurrent = portfolio.current;
    const totalPnL = totalCurrent - totalInvested;
    const totalPnLPercent = totalInvested > 0 ? (totalPnL / totalInvested) * 100 : 0;
    const totalHoldings = holdings.length;
    const positiveHoldings = holdings.filter((h: any) => h.pnl >= 0).length;

    return {
      totalInvested,
      totalCurrent,
      totalPnL,
      totalPnLPercent,
      totalHoldings,
      positiveHoldings,
    };
  });

  chartSeries = computed(() => this.investmentTrendChartOptions().series);

  // Loading States
  isLoading = computed(() => this.portfoliosResource.isLoading());
  isLoadingDetails = computed(() => this.holdingsResource.isLoading());
  isLoadingTransactions = computed(() => this.transactionsResource.isLoading());
  isLoadingChart = computed(() => this.analyticsResource.isLoading());

  private aggregateByYear(history: any[]): { year: string; invested: number }[] {
    const yearMap = new Map<string, number>();

    history.forEach((h) => {
      const year = new Date(h.date).getFullYear().toString();
      yearMap.set(year, h.invested);
    });

    return Array.from(yearMap.entries())
      .map(([year, invested]) => ({ year, invested }))
      .sort((a, b) => parseInt(a.year) - parseInt(b.year));
  }

  private getDefaultFromDate(): Date {
    const date = new Date();
    date.setFullYear(date.getFullYear() - 1);
    return date;
  }

  private getDefaultToDate(): Date {
    return new Date();
  }

  private getEmptyTransactionPage(): TransactionPage {
    return { data: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 };
  }

  selectPortfolio(portfolio: Portfolio) {
    this.selectedPortfolio.set(portfolio);
    // page and showDetails reset automatically via linkedSignal
  }

  // Filter Actions
  applyListFilters() {
    this.transactionsResource.reload();
  }

  resetListFilters() {
    this.listFilterFrom.set(this.getDefaultFromDate());
    this.listFilterTo.set(this.getDefaultToDate());
    this.listFilterSearch.set('');
    // Resource updates automatically due to signal dependencies,
    // but if we want to ensure reload logic matches exactly:
    // If signals change, resource re-fetches cleanly.
  }

  applyChartFilters() {
    // Resource updates automatically when duration/frequency signals change
  }

  resetChartFilters() {
    this.duration.set('ALL');
    this.frequency.set('Daily');
  }

  changeDuration(value: '1M' | '3M' | '6M' | 'YTD' | '1Y' | '3Y' | '5Y' | 'ALL') {
    this.duration.set(value);
  }

  changeFrequency(value: 'Daily' | 'Weekly' | 'Monthly') {
    this.frequency.set(value);
  }

  changePage(delta: number) {
    const target = this.page() + delta;
    if (target < 1 || (this.totalPages() && target > this.totalPages())) return;
    this.page.set(target);
  }

  changePageSize(size: number) {
    this.pageSize.set(size);
    this.page.set(1);
  }

  toggleDetails() {
    this.showDetails.update((v) => !v);
  }

  sortBy(field: keyof Transaction) {
    const currentField = this.sortField();
    const currentDir = this.sortDir();
    const nextDir = currentField === field && currentDir === 'asc' ? 'desc' : 'asc';
    this.sortField.set(field);
    this.sortDir.set(nextDir);
  }

  private sortData(data: Transaction[]) {
    const field = this.sortField();
    const dir = this.sortDir();
    if (!field) return data;

    return [...data].sort((a, b) => {
      const va = a[field];
      const vb = b[field];

      if (field === 'tradeDate') {
        const da = new Date(va as any).getTime();
        const db = new Date(vb as any).getTime();
        return da - db;
      }

      if (typeof va === 'number' && typeof vb === 'number') {
        return dir === 'asc' ? va - vb : vb - va;
      }

      const sa = String(va ?? '');
      const sb = String(vb ?? '');
      return dir === 'asc' ? sa.localeCompare(sb) : sb.localeCompare(sa);
    });
  }

  editTransaction(transaction: Transaction) {
    const dialogRef = this.dialog.open(TransactionEditDialogComponent, {
      width: '600px',
      data: { ...transaction },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.updateTransaction(result);
      }
    });
  }

  private async updateTransaction(transaction: Transaction) {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    try {
      await firstValueFrom(this.portfolioService.updateTransaction(portfolioName, transaction));

      this.snackBar.open('Transaction updated successfully', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });

      // Reload resources
      this.holdingsResource.reload();
      this.transactionsResource.reload();
      this.analyticsResource.reload();
    } catch (error) {
      console.error('Failed to update transaction:', error);
      this.snackBar.open('Failed to update transaction. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  async recalculateHoldings() {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    try {
      await firstValueFrom(this.portfolioService.recalculateHoldings(portfolioName));

      this.snackBar.open('Holdings recalculated successfully', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });

      // Reload resources
      this.holdingsResource.reload();
      this.analyticsResource.reload();
    } catch (error) {
      console.error('Failed to recalculate holdings:', error);
      this.snackBar.open('Failed to recalculate holdings. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  async deleteTransaction(transactionId: string) {
    if (!confirm('Are you sure you want to delete this transaction?')) return;

    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    try {
      await firstValueFrom(this.portfolioService.deleteTransaction(portfolioName, transactionId));

      this.snackBar.open('Transaction deleted successfully', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });

      // Reload resources
      this.transactionsResource.reload();
      this.holdingsResource.reload();
      this.analyticsResource.reload();
    } catch (error) {
      console.error('Failed to delete transaction:', error);
      this.snackBar.open('Failed to delete transaction. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  async clearAllTransactions() {
    const confirmed = confirm(
      'Are you sure you want to delete ALL transactions? This action cannot be undone!'
    );
    if (!confirmed) return;

    const doubleConfirm = confirm(
      'This will permanently delete all transactions and recalculate holdings. Continue?'
    );
    if (!doubleConfirm) return;

    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    try {
      await firstValueFrom(this.portfolioService.clearAllTransactions(portfolioName));

      this.snackBar.open('All transactions cleared successfully', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });

      // Reload resources
      this.transactionsResource.reload();
      this.holdingsResource.reload();
      this.analyticsResource.reload();
    } catch (error) {
      console.error('Failed to clear transactions:', error);
      this.snackBar.open('Failed to clear transactions. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  openAddTransactionDialog() {
    const dialogRef = this.dialog.open(TransactionAddDialogComponent, {
      width: '700px',
    });

    dialogRef.afterClosed().subscribe(async (result: CreateTransactionRequest | undefined) => {
      if (result) {
        await this.addTransaction(result);
      }
    });
  }

  async addTransaction(request: CreateTransactionRequest) {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    try {
      await firstValueFrom(this.portfolioService.addTransaction(portfolioName, request));

      this.snackBar.open('Transaction added successfully', 'Close', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });

      // Reload resources
      this.transactionsResource.reload();
      this.holdingsResource.reload();
      this.analyticsResource.reload();
    } catch (error) {
      console.error('Failed to add transaction:', error);
      this.snackBar.open('Failed to add transaction. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  openUploadDialog() {
    // Create file input dynamically
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls';
    input.onchange = async (e: Event) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        await this.uploadExcelFile(file);
      }
    };
    input.click();
  }

  async uploadExcelFile(file: File) {
    const portfolioName = this.selectedPortfolio()?.name;
    if (!portfolioName) return;

    try {
      const response = await firstValueFrom(
        this.portfolioService.uploadTransactions(portfolioName, file)
      );

      if (response.success && response.data) {
        const data = response.data;
        let message = `Successfully processed ${data.addedCount} transactions`;
        if (data.errors && data.errors.length > 0) {
          message += `. ${data.errors.length} errors occurred.`;
        }

        this.snackBar.open(message, 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
        });

        // Reload resources
        this.transactionsResource.reload();
        this.holdingsResource.reload();
        this.analyticsResource.reload();
      }
    } catch (error: any) {
      console.error('Failed to upload file:', error);
      const errorMessage = error?.error?.message || 'Failed to upload file. Please try again.';
      this.snackBar.open(errorMessage, 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  async downloadTemplate() {
    try {
      const blob = await firstValueFrom(this.portfolioService.downloadTemplate());
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'PortVault_Transaction_Template.xlsx';
      link.click();
      window.URL.revokeObjectURL(url);

      this.snackBar.open('Template downloaded successfully', 'Close', {
        duration: 2000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    } catch (error) {
      console.error('Failed to download template:', error);
      this.snackBar.open('Failed to download template. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    }
  }

  backToList() {
    this.selectedPortfolio.set(null);
  }
}
