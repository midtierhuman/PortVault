import { CommonModule } from '@angular/common';
import { Component, signal, inject, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs'; // Added import
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
import { PortfolioService } from '../../core/services/portfolio.service';
import { Portfolio } from '../../models/portfolio.model';
import { AnalyticsHistory } from '../../models/analytics.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatTableModule,
    NgApexchartsModule,
  ],
})
export class DashboardComponent implements OnInit {
  private portfolioService = inject(PortfolioService);
  private snackBar = inject(MatSnackBar);
  private router = inject(Router);

  portfolios = signal<Portfolio[]>([]);
  isLoading = signal(true);
  overallAnalytics = signal<AnalyticsHistory[]>([]);

  // KPI Signals
  totalInvested = signal(0);
  totalCurrent = signal(0);
  overallPnl = signal(0);
  returnPercentage = signal(0);

  // Chart Signals
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
    chart: { type: 'area', height: 360, toolbar: { show: false } },
    xaxis: { categories: [] },
    yaxis: { labels: { formatter: (val) => `₹${Number(val).toLocaleString('en-IN')}` } },
    dataLabels: { enabled: false },
    stroke: { width: 2, curve: 'smooth' },
    tooltip: { shared: true },
    plotOptions: {},
  });

  // Top performers
  topPerformers = signal<Portfolio[]>([]);
  worstPerformers = signal<Portfolio[]>([]);
  holdingsCount = signal(0);

  ngOnInit() {
    this.loadDashboardData();
  }

  private async loadDashboardData() {
    this.isLoading.set(true);
    try {
      const portfolios = await firstValueFrom(this.portfolioService.getAll());
      this.portfolios.set(portfolios);

      // Calculate KPIs
      const totalInv = portfolios.reduce((sum, p) => sum + p.invested, 0);
      const totalCurr = portfolios.reduce((sum, p) => sum + p.current, 0);
      const pnl = totalCurr - totalInv;
      const returnPct = totalInv > 0 ? (pnl / totalInv) * 100 : 0;

      this.totalInvested.set(totalInv);
      this.totalCurrent.set(totalCurr);
      this.overallPnl.set(pnl);
      this.returnPercentage.set(returnPct);

      // Get top/worst performers
      const sorted = [...portfolios].sort((a, b) => {
        const pnlA = a.current - a.invested;
        const pnlB = b.current - b.invested;
        return pnlB - pnlA;
      });
      this.topPerformers.set(sorted.slice(0, 3));
      this.worstPerformers.set(sorted.slice(-3).reverse());

      // Load overall analytics for chart
      if (portfolios.length > 0) {
        this.loadOverallAnalytics();
      }
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
      this.snackBar.open('Failed to load dashboard data. Please try again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
      });
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadOverallAnalytics() {
    try {
      // Load analytics for the first portfolio to get historical data pattern
      const portfolios = this.portfolios();
      if (portfolios.length > 0) {
        const analytics = await firstValueFrom(
          this.portfolioService.getAnalytics(portfolios[0].name, 'ALL', 'Monthly')
        );
        this.overallAnalytics.set(analytics.history);
        this.buildChart();
      }
    } catch (error) {
      console.error('Failed to load analytics:', error);
    }
  }

  private buildChart() {
    const history = this.overallAnalytics();
    if (!history.length) {
      this.chartSeries.set([]);
      return;
    }

    const categories = history.map((h) => h.date);
    const investedValues = history.map((h) => h.amount);

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

  isGaining = (portfolio: Portfolio) => portfolio.current >= portfolio.invested;
  isLosing = (portfolio: Portfolio) => portfolio.current < portfolio.invested;

  gainingPortfolios() {
    return this.portfolios().filter(this.isGaining).length;
  }

  losingPortfolios() {
    return this.portfolios().filter(this.isLosing).length;
  }

  totalValueAtRisk() {
    return this.portfolios()
      .filter(this.isLosing)
      .reduce((sum, p) => sum + (p.invested - p.current), 0);
  }

  viewPortfolio(portfolioName: string) {
    this.router.navigate(['/portfolios'], { queryParams: { portfolio: portfolioName } });
  }
}
