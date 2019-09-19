using System.Collections.Generic;
using System.IO;

namespace Albacore.ViVe
{
    public static class RtlDataHelpers
    {
        public static byte[] SerializeFeatureConfigurations(List<FeatureConfiguration> configurations)
        {
            byte[] retArray = new byte[configurations.Count * 32];
            using (MemoryStream ms = new MemoryStream(retArray, true))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                    foreach (var thing in configurations)
                    {
                        bw.Write(thing.FeatureId);
                        bw.Write(thing.Group);
                        bw.Write((int)thing.EnabledState);
                        bw.Write(thing.EnabledStateOptions);
                        bw.Write(thing.Variant);
                        bw.Write(thing.VariantPayloadKind);
                        bw.Write(thing.VariantPayload);
                        bw.Write((int)thing.Action);
                    }
            }
            return retArray;
        }

        public static byte[] SerializeFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            byte[] retArray = new byte[subscriptions.Count * 16];
            using (MemoryStream ms = new MemoryStream(retArray, true))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                    foreach (var thing in subscriptions)
                    {
                        bw.Write(thing.FeatureId);
                        bw.Write(thing.ReportingKind);
                        bw.Write(thing.ReportingOptions);
                        bw.Write(thing.ReportingTarget);
                    }
            }
            return retArray;
        }

        public static byte[] SerializeFeatureUsageReport(FeatureUsageReport report)
        {
            byte[] retArray = new byte[8];
            using (MemoryStream ms = new MemoryStream(retArray, true))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(report.FeatureId);
                    bw.Write(report.ReportingKind);
                    bw.Write(report.ReportingOptions);
                }
            }
            return retArray;
        }
    }
}
