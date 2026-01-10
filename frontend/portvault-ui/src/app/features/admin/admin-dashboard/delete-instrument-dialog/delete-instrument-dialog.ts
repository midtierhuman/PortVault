import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialog,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { InstrumentService } from '../../../../core/services/instrument.service';
import { InstrumentDependenciesResponse } from '../../../../models/instrument.model';
import { MigrateInstrumentDialogComponent } from '../migrate-instrument-dialog/migrate-instrument-dialog';

@Component({
  selector: 'app-delete-instrument-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatSnackBarModule,
  ],
  templateUrl: './delete-instrument-dialog.html',
  styleUrl: './delete-instrument-dialog.scss',
})
export class DeleteInstrumentDialogComponent {
  dialogRef = inject(MatDialogRef<DeleteInstrumentDialogComponent>);
  data = inject<{ instrumentId: number; instrumentName: string }>(MAT_DIALOG_DATA);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private instrumentService = inject(InstrumentService);

  isLoading = signal<boolean>(true);
  isDeleting = signal<boolean>(false);
  dependencies = signal<InstrumentDependenciesResponse | null>(null);
  error = signal<string | null>(null);

  constructor() {
    this.loadDependencies();
  }

  private loadDependencies(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.instrumentService.getDependencies(this.data.instrumentId).subscribe({
      next: (response) => {
        this.dependencies.set(response.data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to load dependencies');
        this.isLoading.set(false);
      },
    });
  }

  onDelete(): void {
    const deps = this.dependencies();
    if (!deps || !deps.canDelete) return;

    this.isDeleting.set(true);
    this.error.set(null);

    this.instrumentService.delete(this.data.instrumentId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isDeleting.set(false);
        this.error.set(err.error?.message || 'Failed to delete instrument');
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onMigrate(): void {
    const deps = this.dependencies();
    if (!deps) return;

    const migrateDialogRef = this.dialog.open(MigrateInstrumentDialogComponent, {
      width: '600px',
      data: {
        sourceInstrumentId: this.data.instrumentId,
        sourceInstrumentName: this.data.instrumentName,
        transactionCount: deps.transactionCount,
        holdingCount: deps.holdingCount,
        identifierCount: deps.identifierCount,
      },
    });

    migrateDialogRef.afterClosed().subscribe((migrationResult) => {
      if (migrationResult) {
        this.snackBar.open(migrationResult.message, 'Close', { duration: 5000 });
        // Reload dependencies to check if we can now delete
        this.loadDependencies();
      }
    });
  }
}
