import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InstrumentService } from '../../../../core/services/instrument.service';
import { InstrumentResponse } from '../../../../models/instrument.model';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-migrate-instrument-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './migrate-instrument-dialog.html',
  styleUrl: './migrate-instrument-dialog.scss',
})
export class MigrateInstrumentDialogComponent {
  dialogRef = inject(MatDialogRef<MigrateInstrumentDialogComponent>);
  data = inject<{
    sourceInstrumentId: number;
    sourceInstrumentName: string;
    transactionCount: number;
    holdingCount: number;
    identifierCount: number;
  }>(MAT_DIALOG_DATA);
  private instrumentService = inject(InstrumentService);

  selectedTargetId = signal<number | null>(null);
  isMigrating = signal<boolean>(false);
  error = signal<string | null>(null);

  instrumentsResource = rxResource({
    stream: () => this.instrumentService.getAll(),
  });

  availableInstruments = computed(() => {
    const instruments = this.instrumentsResource.value()?.data || [];
    return instruments.filter((i) => i.id !== this.data.sourceInstrumentId);
  });

  canMigrate = computed(() => this.selectedTargetId() !== null);

  onMigrate(): void {
    const targetId = this.selectedTargetId();
    if (!targetId) return;

    this.isMigrating.set(true);
    this.error.set(null);

    this.instrumentService
      .migrate(this.data.sourceInstrumentId, { targetInstrumentId: targetId })
      .subscribe({
        next: (response) => {
          this.isMigrating.set(false);
          this.dialogRef.close(response.data);
        },
        error: (err) => {
          this.isMigrating.set(false);
          this.error.set(err.error?.message || 'Failed to migrate instrument');
        },
      });
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}
