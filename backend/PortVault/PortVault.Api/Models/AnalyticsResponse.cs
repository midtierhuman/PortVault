namespace PortVault.Api.Models
{
    public class AnalyticsResponse
    {
        public List<TimePoint> History { get; set; } = new();
        public List<AllocationPoint> SegmentAllocation { get; set; } = new();
    }

    public class TimePoint
    {
        public DateTime Date { get; set; }
        public decimal Invested { get; set; }
    }

    public class AllocationPoint
    {
        public string Segment { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }
}
