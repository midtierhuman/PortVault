export enum CorporateActionType {
  Split = 'Split',
  Bonus = 'Bonus',
  Merger = 'Merger',
  Demerger = 'Demerger',
  NameChange = 'NameChange',
}

export interface CorporateActionResponse {
  id: number;
  type: string;
  exDate: string;
  parentInstrumentId: number;
  parentInstrumentName: string;
  childInstrumentId?: number;
  childInstrumentName?: string;
  ratioNumerator: number;
  ratioDenominator: number;
  costPercentageAllocated: number;
}

export interface CreateCorporateActionRequest {
  type: string;
  exDate: string;
  parentInstrumentId: number;
  childInstrumentId?: number;
  ratioNumerator: number;
  ratioDenominator: number;
  costPercentageAllocated: number;
}

export interface UpdateCorporateActionRequest {
  type: string;
  exDate: string;
  parentInstrumentId: number;
  childInstrumentId?: number;
  ratioNumerator: number;
  ratioDenominator: number;
  costPercentageAllocated: number;
}
