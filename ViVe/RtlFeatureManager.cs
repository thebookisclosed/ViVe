using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Albacore.ViVe
{
    public static class RtlFeatureManager
    {
        public static List<FeatureConfiguration> QueryAllFeatureConfigurations()
        {
            uint dummy = 0;
            return QueryAllFeatureConfigurations(FeatureConfigurationSection.Runtime, ref dummy);
        }

        public static List<FeatureConfiguration> QueryAllFeatureConfigurations(FeatureConfigurationSection section)
        {
            uint dummy = 0;
            return QueryAllFeatureConfigurations(section, ref dummy);
        }

        public static List<FeatureConfiguration> QueryAllFeatureConfigurations(FeatureConfigurationSection section, ref uint changeStamp)
        {
            int featureCount = 0;
            NativeMethods.RtlQueryAllFeatureConfigurations(section, ref changeStamp, IntPtr.Zero, ref featureCount);
            if (featureCount == 0)
                return null;
            // One feature config is 12 bytes long
            IntPtr rawBuf = Marshal.AllocHGlobal(featureCount * 12);
            int hRes = NativeMethods.RtlQueryAllFeatureConfigurations(section, ref changeStamp, rawBuf, ref featureCount);
            if (hRes != 0)
            {
                Marshal.FreeHGlobal(rawBuf);
                return null;
            }
            byte[] buf = new byte[featureCount * 12];
            Marshal.Copy(rawBuf, buf, 0, buf.Length);
            Marshal.FreeHGlobal(rawBuf);
            var allFeatureConfigs = new List<FeatureConfiguration>();
            using (MemoryStream ms = new MemoryStream(buf, false))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    for (int i = 0; i < featureCount; i++)
                    {
                        uint featureId = br.ReadUInt32();
                        int compactState = br.ReadInt32();

                        allFeatureConfigs.Add(new FeatureConfiguration() {
                            FeatureId = featureId,
                            Group = compactState & 0xF,
                            EnabledState = (FeatureEnabledState)((compactState & 0x30) >> 4),
                            EnabledStateOptions = (compactState & 0x40) >> 6,
                            Variant = (compactState & 0x3F00) >> 8,
                            VariantPayloadKind = (compactState & 0xC000) >> 14,
                            VariantPayload = br.ReadInt32() });
                    }
                }
            }
            return allFeatureConfigs;
        }

        public static FeatureConfiguration QueryFeatureConfiguration(uint featureId)
        {
            uint dummy = 0;
            return QueryFeatureConfiguration(featureId, FeatureConfigurationSection.Runtime, ref dummy);
        }

        public static FeatureConfiguration QueryFeatureConfiguration(uint featureId, FeatureConfigurationSection section)
        {
            uint dummy = 0;
            return QueryFeatureConfiguration(featureId, section, ref dummy);
        }

        public static FeatureConfiguration QueryFeatureConfiguration(uint featureId, FeatureConfigurationSection section, ref uint changeStamp)
        {
            // One feature config is 12 bytes long
            IntPtr rawBuf = Marshal.AllocHGlobal(12);
            int hRes = NativeMethods.RtlQueryFeatureConfiguration(featureId, section, ref changeStamp, rawBuf);
            if (hRes != 0)
            {
                Marshal.FreeHGlobal(rawBuf);
                return null;
            }
            byte[] buf = new byte[12];
            Marshal.Copy(rawBuf, buf, 0, buf.Length);
            Marshal.FreeHGlobal(rawBuf);
            int compactState = BitConverter.ToInt32(buf, 4);
            return new FeatureConfiguration() {
                FeatureId = BitConverter.ToUInt32(buf, 0),
                Group = compactState & 0xF,
                EnabledState = (FeatureEnabledState)((compactState & 0x30) >> 4),
                EnabledStateOptions = (compactState & 0x40) >> 6,
                Variant = (compactState & 0x3F00) >> 8,
                VariantPayloadKind = (compactState & 0xC000) >> 14,
                VariantPayload = BitConverter.ToInt32(buf, 8) };
        }

        public static uint QueryFeatureConfigurationChangeStamp()
        {
            return NativeMethods.RtlQueryFeatureConfigurationChangeStamp();
        }

        public static int SetFeatureConfigurations(List<FeatureConfiguration> configurations)
        {
            uint dummy = 0;
            return SetFeatureConfigurations(configurations, FeatureConfigurationSection.Runtime, ref dummy);
        }

        public static int SetFeatureConfigurations(List<FeatureConfiguration> configurations, FeatureConfigurationSection section)
        {
            uint dummy = 0;
            return SetFeatureConfigurations(configurations, section, ref dummy);
        }

        public static int SetFeatureConfigurations(List<FeatureConfiguration> configurations, FeatureConfigurationSection section, ref uint changeStamp)
        {
            return NativeMethods.RtlSetFeatureConfigurations(ref changeStamp, section, RtlDataHelpers.SerializeFeatureConfigurations(configurations), configurations.Count);
        }

        public static IntPtr RegisterFeatureConfigurationChangeNotification(FeatureConfigurationChangeCallback callback)
        {
            return RegisterFeatureConfigurationChangeNotification(callback, IntPtr.Zero);
        }

        public static IntPtr RegisterFeatureConfigurationChangeNotification(FeatureConfigurationChangeCallback callback, IntPtr context)
        {
            NativeMethods.RtlRegisterFeatureConfigurationChangeNotification(callback, context, IntPtr.Zero, out IntPtr sub);
            return sub;
        }

        public static int UnregisterFeatureConfigurationChangeNotification(IntPtr notification)
        {
            return NativeMethods.RtlUnregisterFeatureConfigurationChangeNotification(notification);
        }

        public static List<FeatureUsageSubscription> QueryFeatureUsageSubscriptions()
        {
            int subCount = 0;
            NativeMethods.RtlQueryFeatureUsageNotificationSubscriptions(IntPtr.Zero, ref subCount);
            if (subCount == 0)
                return null;
            // One feature subscription is 16 bytes long
            IntPtr rawBuf = Marshal.AllocHGlobal(subCount * 16);
            int hRes = NativeMethods.RtlQueryFeatureUsageNotificationSubscriptions(rawBuf, ref subCount);
            if (hRes != 0)
                return null;
            byte[] buf = new byte[subCount * 16];
            Marshal.Copy(rawBuf, buf, 0, buf.Length);
            Marshal.FreeHGlobal(rawBuf);
            var allSubscriptions = new List<FeatureUsageSubscription>();
            using (MemoryStream ms = new MemoryStream(buf, false))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    for (int i = 0; i < subCount; i++)
                    {
                        allSubscriptions.Add(new FeatureUsageSubscription() {
                            FeatureId = br.ReadUInt32(),
                            ReportingKind = br.ReadUInt16(),
                            ReportingOptions = br.ReadUInt16(),
                            ReportingTarget = br.ReadUInt64()
                        });
                    }
                }
            }
            return allSubscriptions;
        }

        public static int AddFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            return NativeMethods.RtlSubscribeForFeatureUsageNotification(RtlDataHelpers.SerializeFeatureUsageSubscriptions(subscriptions), subscriptions.Count);
        }

        public static int RemoveFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            return NativeMethods.RtlUnsubscribeFromFeatureUsageNotifications(RtlDataHelpers.SerializeFeatureUsageSubscriptions(subscriptions), subscriptions.Count);
        }

        public static int NotifyFeatureUsage(FeatureUsageReport report)
        {
            return NativeMethods.RtlNotifyFeatureUsage(RtlDataHelpers.SerializeFeatureUsageReport(report));
        }
    }
}
