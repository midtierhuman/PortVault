// Restrict trade types to API-aligned values.
export enum TradeType {
  Buy = 'Buy',
  Sell = 'Sell',
}

export interface Transaction {
  id: string;
  symbol: string;
  isin: string;
  tradeDate: string;
  orderExecutionTime: string;
  segment: string;
  series: string;
  tradeType: TradeType;
  quantity: number;
  price: number;
  tradeID: number;
  orderID: number;
}

export interface TransactionPage {
  data: Transaction[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateTransactionRequest {
  symbol: string;
  isin: string;
  tradeDate: Date;
  orderExecutionTime?: Date;
  segment: string;
  series: string;
  tradeType: string;
  quantity: number;
  price: number;
  tradeID?: string;
  orderID?: string;
}

export interface TransactionUploadResponse {
  message: string;
  totalProcessed: number;
  newTransactions?: number;
  addedCount: number;
  errors: string[];
}
