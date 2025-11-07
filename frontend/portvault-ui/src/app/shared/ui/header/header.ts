import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router } from '@angular/router';

export type NavBtn = { label: string; route: string };

@Component({
  selector: 'pv-header',
  standalone: true,
  imports: [CommonModule, MatToolbarModule, MatButtonModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  @Input() navBtns: NavBtn[] = [];
  constructor(private router: Router) {}
  go(path: string) {
    this.router.navigate([path]);
  }
}
