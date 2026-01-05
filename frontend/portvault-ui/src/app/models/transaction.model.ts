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
