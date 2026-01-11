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
import { MatTabsModule } from '@angular/material/tabs';
import { rxResource } from '@angular/core/rxjs-interop';
import { InstrumentService } from '../../../core/services/instrument.service';
import { CorporateActionService } from '../../../core/services/corporate-action.service';
import {
  InstrumentResponse,
  InstrumentType,
  IdentifierType,
  CreateInstrumentRequest,
  UpdateInstrumentRequest,
  AddInstrumentIdentifierRequest,
} from '../../../models/instrument.model';
import {
  CorporateActionResponse,
  CorporateActionType,
} from '../../../models/corporate-action.model';
import { InstrumentDialogComponent } from './instrument-dialog/instrument-dialog';
import { IdentifierDialogComponent } from './identifier-dialog/identifier-dialog';
import { DeleteInstrumentDialogComponent } from './delete-instrument-dialog/delete-instrument-dialog';
import { CorporateActionDialogComponent } from './corporate-action-dialog/corporate-action-dialog';

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
    MatTabsModule,
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboardComponent {
  private readonly instrumentService = inject(InstrumentService);
  private readonly corporateActionService = inject(CorporateActionService);
  private readonly dialog = inject(MatDialog);

  readonly searchQuery = signal<string>('');
  readonly displayedColumns = ['id', 'type', 'name', 'identifiers', 'actions'] as const;
  readonly corporateActionColumns = [
    'id',
    'type',
    'exDate',
    'parentInstrument',
    'childInstrument',
    'ratio',
    'costPercentage',
    'actions',
  ] as const;

  readonly instrumentsResource = rxResource({
    params: () => this.searchQuery(),
    stream: ({ params }) => this.instrumentService.getAll(params || undefined),
  });

  readonly corporateActionsResource = rxResource({
    stream: () => this.corporateActionService.getAll(),
  });

  readonly InstrumentType = InstrumentType;
  readonly IdentifierType = IdentifierType;
  readonly CorporateActionType = CorporateActionType;

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

  // Corporate Action Methods
  openCreateCorporateActionDialog(): void {
    const dialogRef = this.dialog.open(CorporateActionDialogComponent, {
      width: '600px',
      data: null,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.corporateActionsResource.reload();
      }
    });
  }

  openEditCorporateActionDialog(corporateAction: CorporateActionResponse): void {
    const dialogRef = this.dialog.open(CorporateActionDialogComponent, {
      width: '600px',
      data: corporateAction,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.corporateActionsResource.reload();
      }
    });
  }

  deleteCorporateAction(id: number): void {
    if (confirm('Are you sure you want to delete this corporate action?')) {
      this.corporateActionService.delete(id).subscribe(() => {
        this.corporateActionsResource.reload();
      });
    }
  }

  formatRatio(numerator: number, denominator: number): string {
    return `${numerator}:${denominator}`;
  }
}
