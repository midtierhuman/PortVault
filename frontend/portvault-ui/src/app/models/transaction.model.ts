import { tradeType } from './trade-type.model';

export interface Transaction {
  id: string;
  instrumentId: string;
  type: tradeType;
  date: Date;
  price: number;
  qty: number;
}
