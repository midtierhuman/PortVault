import { Component, inject, signal, computed, effect } from '@angular/core';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormControl,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { CorporateActionService } from '../../../../core/services/corporate-action.service';
import { InstrumentService } from '../../../../core/services/instrument.service';
import {
  CorporateActionType,
  CorporateActionResponse,
} from '../../../../models/corporate-action.model';
import { InstrumentResponse } from '../../../../models/instrument.model';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-corporate-action-dialog',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatAutocompleteModule,
  ],
  templateUrl: './corporate-action-dialog.html',
  styleUrl: './corporate-action-dialog.scss',
})
export class CorporateActionDialogComponent {
  readonly dialogRef = inject(MatDialogRef<CorporateActionDialogComponent>);
  private readonly data = inject<CorporateActionResponse | null>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);
  private readonly corporateActionService = inject(CorporateActionService);
  private readonly instrumentService = inject(InstrumentService);

  // Signals
  readonly isEditMode = signal<boolean>(!!this.data);
  readonly isLoading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  readonly corporateActionTypes = Object.values(CorporateActionType);

  // Resource for loading instruments
  readonly instrumentsResource = rxResource({
    stream: () => this.instrumentService.getAll(),
  });

  // Computed signals for derived state
  readonly instruments = computed(() => this.instrumentsResource.value()?.data ?? []);

  // Typed FormGroup
  readonly form: FormGroup<{
    type: FormControl<string | null>;
    exDate: FormControl<Date | string | null>;
    parentInstrumentId: FormControl<number | null>;
    childInstrumentId: FormControl<number | null>;
    ratioNumerator: FormControl<number | null>;
    ratioDenominator: FormControl<number | null>;
    costPercentageAllocated: FormControl<number | null>;
  }> = this.fb.group({
    type: [this.data?.type ?? '', Validators.required],
    exDate: [this.data ? new Date(this.data.exDate) : '', Validators.required],
    parentInstrumentId: [this.data?.parentInstrumentId ?? null, Validators.required],
    childInstrumentId: [this.data?.childInstrumentId ?? null],
    ratioNumerator: [
      this.data?.ratioNumerator ?? 1,
      [Validators.required, Validators.min(0.000001)],
    ],
    ratioDenominator: [
      this.data?.ratioDenominator ?? 1,
      [Validators.required, Validators.min(0.000001)],
    ],
    costPercentageAllocated: [
      this.data?.costPercentageAllocated ?? 0,
      [Validators.required, Validators.min(0), Validators.max(100)],
    ],
  });

  readonly selectedType = computed(() => this.form.controls.type.value ?? '');
  readonly requiresChildInstrument = computed(() => {
    const type = this.selectedType();
    return type === 'Merger' || type === 'Demerger';
  });

  constructor() {
    // Use effect to handle dynamic validation based on type changes
    effect(() => {
      const type = this.selectedType();
      const childControl = this.form.controls.childInstrumentId;

      if (type === 'Merger' || type === 'Demerger') {
        childControl.setValidators([Validators.required]);
      } else {
        childControl.setValidators([]);
      }
      childControl.updateValueAndValidity({ emitEvent: false });
    });
  }

  getInstrumentName(id: number | null): string {
    if (!id) return '';
    const instrument = this.instruments().find((i) => i.id === id);
    return instrument?.name ?? '';
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.error.set(null);

    const formValue = this.form.getRawValue();
    const exDateValue = formValue.exDate;

    const request = {
      type: formValue.type!,
      exDate: exDateValue instanceof Date ? exDateValue.toISOString() : exDateValue!,
      parentInstrumentId: formValue.parentInstrumentId!,
      childInstrumentId: formValue.childInstrumentId ?? undefined,
      ratioNumerator: formValue.ratioNumerator!,
      ratioDenominator: formValue.ratioDenominator!,
      costPercentageAllocated: formValue.costPercentageAllocated!,
    };

    const operation = this.isEditMode()
      ? this.corporateActionService.update(this.data!.id, request)
      : this.corporateActionService.create(request);

    operation.subscribe({
      next: () => {
        this.isLoading.set(false);
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message ?? 'An error occurred');
      },
    });
  }
}
