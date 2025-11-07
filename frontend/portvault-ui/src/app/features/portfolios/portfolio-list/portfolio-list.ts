import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';

@Component({
  selector: 'app-portfolio-list',
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule],
  templateUrl: './portfolio-list.html',
  styleUrl: './portfolio-list.scss',
})
export class PortfolioList {
  constructor(private router: Router) {}

  portfolios: Portfolio[] = [
    { id: 'p1', name: 'LargeCap', invested: 100000, current: 132500 },
    { id: 'p2', name: 'SmallCap + MidCap', invested: 80000, current: 92000 },
  ];

  open(id: string) {
    this.router.navigate(['/portfolios', id]);
  }
}
