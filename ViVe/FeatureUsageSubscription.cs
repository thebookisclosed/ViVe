namespace Albacore.ViVe
{
    public class FeatureUsageSubscription
    {
        public uint FeatureId { get; set; }
        public ushort ReportingKind { get; set; }
        public ushort ReportingOptions { get; set; }
        public ulong ReportingTarget { get; set; }
    }
}
