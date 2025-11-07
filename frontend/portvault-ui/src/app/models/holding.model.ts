import { AssetType } from './asset-type.model';

export interface Holding {
  portfolioId: string;
  instrumentId: string;
  name: string;
  type: AssetType;
  qty: number;
  avgPrice: number;
}
