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
import { InstrumentService } from '../../../../core/services/instrument.service';
import { InstrumentType, InstrumentResponse } from '../../../../models/instrument.model';

@Component({
  selector: 'app-instrument-dialog',
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
  ],
  templateUrl: './instrument-dialog.html',
  styleUrl: './instrument-dialog.scss',
})
export class InstrumentDialogComponent {
  dialogRef = inject(MatDialogRef<InstrumentDialogComponent>);
  data = inject<InstrumentResponse | null>(MAT_DIALOG_DATA);
  private fb = inject(FormBuilder);
  private instrumentService = inject(InstrumentService);

  isEditMode = signal<boolean>(!!this.data);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  instrumentTypes = Object.values(InstrumentType);

  form: FormGroup;

  constructor() {
    this.form = this.fb.group({
      type: [this.data?.type || '', Validators.required],
      name: [this.data?.name || '', [Validators.required, Validators.maxLength(200)]],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.error.set(null);

    const request = this.form.value;
    const operation = this.isEditMode()
      ? this.instrumentService.update(this.data!.id, request)
      : this.instrumentService.create(request);

    operation.subscribe({
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
