import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  standalone: true,
  imports: [CommonModule, MatCardModule],
})
export class DashboardComponent {
  totalInvested = 215000;
  currentValue = 254400;
  get pnl() {
    return this.currentValue - this.totalInvested;
  }

  holdings = [
    { symbol: 'NIFTY50', value: 82000 },
    { symbol: 'HDFCAMC', value: 42000 },
    { symbol: 'NIMF', value: 130000 },
  ];
}
