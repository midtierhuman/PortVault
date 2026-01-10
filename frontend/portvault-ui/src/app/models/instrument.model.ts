export enum InstrumentType {
  MF = 'MF',
  EQ = 'EQ',
}

export enum IdentifierType {
  ISIN = 'ISIN',
  TICKER = 'TICKER',
  NSE_SYMBOL = 'NSE_SYMBOL',
  BSE_CODE = 'BSE_CODE',
  SCHEME_CODE = 'SCHEME_CODE',
}

export interface InstrumentIdentifierResponse {
  id: number;
  type: IdentifierType;
  value: string;
  validFrom?: string;
  validTo?: string;
}

export interface InstrumentResponse {
  id: number;
  type: InstrumentType;
  name: string;
  identifiers: InstrumentIdentifierResponse[];
}

export interface CreateInstrumentRequest {
  type: InstrumentType;
  name: string;
}

export interface UpdateInstrumentRequest {
  type: InstrumentType;
  name: string;
}

export interface AddInstrumentIdentifierRequest {
  type: IdentifierType;
  value: string;
  validFrom?: string;
  validTo?: string;
}

export interface InstrumentDependenciesResponse {
  instrumentId: number;
  instrumentName: string;
  canDelete: boolean;
  transactionCount: number;
  holdingCount: number;
  identifierCount: number;
  identifiers: InstrumentIdentifierResponse[];
  message: string;
}

export interface MigrateInstrumentRequest {
  targetInstrumentId: number;
}

export interface InstrumentMigrationResponse {
  sourceInstrumentId: number;
  sourceInstrumentName: string;
  targetInstrumentId: number;
  targetInstrumentName: string;
  identifiersMoved: number;
  transactionsMigrated: number;
  holdingsMigrated: number;
  message: string;
}
