import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { CreateTransactionRequest, TradeType } from '../../../../models/transaction.model';

@Component({
  selector: 'app-transaction-add-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './transaction-add-dialog.html',
  styleUrls: ['./transaction-add-dialog.scss'],
})
export class TransactionAddDialogComponent {
  readonly dialogRef = inject(MatDialogRef<TransactionAddDialogComponent>);

  readonly tradeTypes: TradeType[] = [TradeType.Buy, TradeType.Sell];
  readonly segments = ['EQ', 'FO', 'CD', 'CO', 'MF'];

  readonly transactionForm = new FormGroup<{
    symbol: FormControl<string>;
    isin: FormControl<string>;
    tradeDate: FormControl<Date | null>;
    orderExecutionTime: FormControl<Date | null>;
    segment: FormControl<string>;
    series: FormControl<string>;
    tradeType: FormControl<string>;
    quantity: FormControl<number | null>;
    price: FormControl<number | null>;
    tradeID: FormControl<string>;
    orderID: FormControl<string>;
  }>({
    symbol: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    isin: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(50)],
    }),
    tradeDate: new FormControl<Date | null>(null, {
      validators: [Validators.required],
    }),
    orderExecutionTime: new FormControl<Date | null>(null),
    segment: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(50)],
    }),
    series: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.maxLength(50)],
    }),
    tradeType: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    quantity: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(0.000001)],
    }),
    price: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(0.000001)],
    }),
    tradeID: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
    orderID: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)],
    }),
  });

  onSave() {
    if (this.transactionForm.invalid) {
      this.transactionForm.markAllAsTouched();
      return;
    }

    const formValue = this.transactionForm.value;
    const request: CreateTransactionRequest = {
      symbol: formValue.symbol!,
      isin: formValue.isin!,
      tradeDate: formValue.tradeDate!,
      orderExecutionTime: formValue.orderExecutionTime ?? undefined,
      segment: formValue.segment!,
      series: formValue.series!,
      tradeType: formValue.tradeType!,
      quantity: formValue.quantity!,
      price: formValue.price!,
      tradeID: formValue.tradeID || undefined,
      orderID: formValue.orderID || undefined,
    };

    this.dialogRef.close(request);
  }

  onCancel() {
    this.dialogRef.close();
  }
}
