import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PortfolioDetails } from './portfolio-details';

describe('PortfolioDetails', () => {
  let component: PortfolioDetails;
  let fixture: ComponentFixture<PortfolioDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortfolioDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PortfolioDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
