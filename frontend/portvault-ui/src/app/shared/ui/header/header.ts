import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

export type NavBtn = { label: string; route: string };

@Component({
  selector: 'pv-header',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  @Input() navBtns: NavBtn[] = [];
  private router = inject(Router);
  authService = inject(AuthService);

  go(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.authService.logout();
  }
}
