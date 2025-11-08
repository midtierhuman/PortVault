import { Component, Inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { Asset } from '../../../../models/asset.model';
import { Transaction } from '../../../../models/transaction.model';
import { tradeType } from '../../../../models/trade-type.model';

@Component({
  standalone: true,
  selector: 'app-transaction-dialog',
  imports: [
    CommonModule,
    MatDialogModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './transaction-dialog.html',
  styleUrl: './transaction-dialog.scss',
})
export class TransactionDialogComponent {
  transactions = signal<Transaction[]>([]);

  constructor(@Inject(MAT_DIALOG_DATA) public data: Asset) {}

  ngOnInit() {
    this.addBlank();
  }

  addBlank() {
    this.transactions.update((list) => [
      ...list,
      {
        id: '',
        instrumentId: this.data.instrumentId,
        type: tradeType.Buy,
        date: new Date(),
        price: 0,
        qty: 0,
      },
    ]);
  }

  updateField(i: number, field: keyof Transaction, value: any) {
    if (field === 'date') value = new Date(value);
    this.transactions.update((list) => {
      const copy = structuredClone(list);
      copy[i] = { ...copy[i], [field]: value };
      return copy;
    });
  }

  save() {
    console.log(this.transactions());
  }
}
