import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { InstrumentService } from '../../../../core/services/instrument.service';
import { IdentifierType } from '../../../../models/instrument.model';

@Component({
  selector: 'app-identifier-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
  ],
  templateUrl: './identifier-dialog.html',
  styleUrl: './identifier-dialog.scss',
})
export class IdentifierDialogComponent {
  dialogRef = inject(MatDialogRef<IdentifierDialogComponent>);
  data = inject<{ instrumentId: number }>(MAT_DIALOG_DATA);
  private fb = inject(FormBuilder);
  private instrumentService = inject(InstrumentService);

  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  identifierTypes = Object.values(IdentifierType);

  form: FormGroup;

  constructor() {
    this.form = this.fb.group({
      type: ['', Validators.required],
      value: ['', [Validators.required, Validators.maxLength(100)]],
      validFrom: [null],
      validTo: [null],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.error.set(null);

    const formValue = this.form.value;
    const request = {
      type: formValue.type,
      value: formValue.value,
      validFrom: formValue.validFrom ? formValue.validFrom.toISOString() : undefined,
      validTo: formValue.validTo ? formValue.validTo.toISOString() : undefined,
    };

    this.instrumentService.addIdentifier(this.data.instrumentId, request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || 'An error occurred');
      },
    });
  }
}
