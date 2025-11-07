import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { PortfolioDetails } from '../../../models/portfolio-details.model';
import { CommonModule } from '@angular/common';
import { AssetType } from '../../../models/asset-type.model';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { Asset } from '../../../models/asset.model';
import { MatDialog } from '@angular/material/dialog';
import { TransactionDialogComponent } from './transaction-dialog/transaction-dialog';

@Component({
  selector: 'app-portfolio-details',
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule],
  templateUrl: './portfolio-details.html',
  styleUrl: './portfolio-details.scss',
  standalone: true,
})
export class PortfolioDetailsComponent {
  id: string | null = null;
  AssetType = AssetType;

  portfolioDetails = signal<PortfolioDetails | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private portfolioService: PortfolioService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.fetchPortfolioDetails(this.id);
    } else {
      this.router.navigate(['../']);
    }
  }

  private fetchPortfolioDetails(id: string) {
    this.portfolioService.getOne(id).then((p) => {
      this.portfolioDetails.set(p);
    });
  }

  openTransactions(h: Asset) {
    this.dialog.open(TransactionDialogComponent, { data: h });
  }
}
