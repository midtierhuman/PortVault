export interface AnalyticsHistory {
  date: string;
  invested: number;
}

export interface SegmentAllocation {
  segment: string;
  value: number;
  percentage: number;
}

export interface PortfolioAnalytics {
  history: AnalyticsHistory[];
  segmentAllocation: SegmentAllocation[];
}
