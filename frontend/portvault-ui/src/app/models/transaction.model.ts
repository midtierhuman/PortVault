export interface Transaction {
  id: string;
  instrumentId: string;
  type: 'buy' | 'sell';
  date: Date;
  price: number;
  qty: number;
}
