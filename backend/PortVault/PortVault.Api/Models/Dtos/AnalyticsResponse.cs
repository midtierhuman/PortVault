namespace PortVault.Api.Models.Dtos
{
    public class AnalyticsResponse
    {
        public List<TimePoint> History { get; set; } = new();
        public List<AllocationPoint> SegmentAllocation { get; set; } = new();
        public string ViewType { get; set; } = "cumulative";
    }

    public class TimePoint
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class AllocationPoint
    {
        public string Segment { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
    }
}
