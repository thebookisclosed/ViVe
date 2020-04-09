/*
    ViVe - Vibranium feature configuration library
    Copyright (C) 2019  @thebookisclosed

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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

        private static uint SwapBytes(uint x)
        {
            x = (x >> 16) | (x << 16);
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        private static uint RotateRight32(uint value, int shift)
        {
            return (value >> shift) | (value << (32 - shift));
        }

        public static uint GetObfuscatedFeatureId(uint featureId)
        {
            return RotateRight32(SwapBytes(featureId ^ 0x74161A4E) ^ 0x8FB23D4F, -1) ^ 0x833EA8FF;
        }
    }
}
