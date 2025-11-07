import { AssetType } from './asset-type.model';

export interface Asset {
  instrumentId: string; // ISIN or SYMBOL
  type: AssetType;
  name: string;

  nav?: number;
  inav?: number;
  marketPrice: number;
  lastUpdated: Date;
}
