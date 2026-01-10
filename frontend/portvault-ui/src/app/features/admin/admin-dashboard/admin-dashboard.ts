import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  template: `
    <div class="admin-dashboard-container">
      <div class="header">
        <h1>Admin Dashboard</h1>
        <p>System overview and administration.</p>
      </div>

      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-card-header>
            <mat-card-title>Total Users</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stat-value">{{ totalUsers() }}</div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-header>
            <mat-card-title>System Health</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stat-value status-ok">OK</div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [
    `
      .admin-dashboard-container {
        padding: 24px;
        max-width: 1200px;
        margin: 0 auto;
      }
      .header {
        margin-bottom: 32px;
      }
      .stats-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
        gap: 24px;
      }
      .stat-card {
        padding: 16px;
      }
      .stat-value {
        font-size: 3rem;
        font-weight: 500;
        margin-top: 16px;
        color: var(--primary-color, #3f51b5);
      }
      .status-ok {
        color: #4caf50;
      }
    `,
  ],
})
export class AdminDashboardComponent {
  totalUsers = signal(150); // Mock data for now
}
