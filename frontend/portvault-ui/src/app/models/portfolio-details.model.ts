import { Asset } from './asset.model';

export interface PortfolioDetails {
  id: string;
  name: string;

  holdings: Asset[];
}
