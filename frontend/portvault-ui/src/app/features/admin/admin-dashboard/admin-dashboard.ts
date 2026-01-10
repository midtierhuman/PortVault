import { CommonModule } from '@angular/common';
import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { InstrumentService } from '../../../core/services/instrument.service';
import {
  InstrumentResponse,
  InstrumentType,
  IdentifierType,
  CreateInstrumentRequest,
  UpdateInstrumentRequest,
  AddInstrumentIdentifierRequest,
} from '../../../models/instrument.model';
import { InstrumentDialogComponent } from './instrument-dialog/instrument-dialog';
import { IdentifierDialogComponent } from './identifier-dialog/identifier-dialog';
import { DeleteInstrumentDialogComponent } from './delete-instrument-dialog/delete-instrument-dialog';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboardComponent {
  private instrumentService = inject(InstrumentService);
  private dialog = inject(MatDialog);

  searchQuery = signal<string>('');
  displayedColumns = ['id', 'type', 'name', 'identifiers', 'actions'];

  instrumentsResource = rxResource({
    params: () => this.searchQuery(),
    stream: ({ params }) => this.instrumentService.getAll(params || undefined),
  });

  InstrumentType = InstrumentType;
  IdentifierType = IdentifierType;

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(InstrumentDialogComponent, {
      width: '500px',
      data: null,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.instrumentsResource.reload();
      }
    });
  }

  openEditDialog(instrument: InstrumentResponse): void {
    const dialogRef = this.dialog.open(InstrumentDialogComponent, {
      width: '500px',
      data: instrument,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.instrumentsResource.reload();
      }
    });
  }

  openAddIdentifierDialog(instrument: InstrumentResponse): void {
    const dialogRef = this.dialog.open(IdentifierDialogComponent, {
      width: '500px',
      data: { instrumentId: instrument.id },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.instrumentsResource.reload();
      }
    });
  }

  deleteIdentifier(identifierId: number): void {
    if (confirm('Are you sure you want to delete this identifier?')) {
      this.instrumentService.deleteIdentifier(identifierId).subscribe(() => {
        this.instrumentsResource.reload();
      });
    }
  }

  openDeleteDialog(instrument: InstrumentResponse): void {
    const dialogRef = this.dialog.open(DeleteInstrumentDialogComponent, {
      width: '600px',
      data: { instrumentId: instrument.id, instrumentName: instrument.name },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.instrumentsResource.reload();
      }
    });
  }
}
