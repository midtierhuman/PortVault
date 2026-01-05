import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TransactionEditDialogComponent } from './transaction-edit-dialog';
import { TradeType, Transaction } from '../../../../models/transaction.model';

describe('TransactionEditDialogComponent', () => {
  let component: TransactionEditDialogComponent;
  let fixture: ComponentFixture<TransactionEditDialogComponent>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<TransactionEditDialogComponent>>;

  const mockTransaction: Transaction = {
    id: '1aba99fa-c4bd-4df4-a6c2-faa06a5183bb',
    symbol: 'GOLDBEES',
    isin: 'INF204KB17I5',
    tradeDate: '2025-12-31T00:00:00',
    orderExecutionTime: '2025-12-31T13:00:47',
    segment: 'EQ',
    series: 'EQ',
    tradeType: TradeType.Buy,
    quantity: 1,
    price: 110.11,
    tradeID: 206119606,
    orderID: 1100000048903995,
  };

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [TransactionEditDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: mockTransaction },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TransactionEditDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with transaction data', () => {
    expect(component.transaction().symbol).toBe('GOLDBEES');
    expect(component.transaction().quantity).toBe(1);
  });

  it('should close dialog with transaction on save', () => {
    component.onSave();
    expect(mockDialogRef.close).toHaveBeenCalledWith(component.transaction());
  });

  it('should close dialog without data on cancel', () => {
    component.onCancel();
    expect(mockDialogRef.close).toHaveBeenCalledWith();
  });
});
