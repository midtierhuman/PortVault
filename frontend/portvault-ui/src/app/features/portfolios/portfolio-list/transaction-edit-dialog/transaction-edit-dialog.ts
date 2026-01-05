import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Transaction, TradeType } from '../../../../models/transaction.model';

@Component({
  selector: 'app-transaction-edit-dialog',
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './transaction-edit-dialog.html',
  styleUrls: ['./transaction-edit-dialog.scss'],
})
export class TransactionEditDialogComponent {
  dialogRef = inject(MatDialogRef<TransactionEditDialogComponent>);
  data: Transaction = inject(MAT_DIALOG_DATA);

  transaction = signal<Transaction>({ ...this.data });

  tradeTypes: TradeType[] = [TradeType.Buy, TradeType.Sell];
  segments = ['EQ', 'FO', 'CD', 'CO', 'MF'];

  onSave() {
    this.dialogRef.close(this.transaction());
  }

  onCancel() {
    this.dialogRef.close();
  }

  updateField<K extends keyof Transaction>(field: K, value: Transaction[K]) {
    this.transaction.update((t) => ({ ...t, [field]: value }));
  }
}
