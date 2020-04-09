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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static int SetLiveFeatureConfigurations(List<FeatureConfiguration> configurations)
        {
            uint dummy = 0;
            return SetLiveFeatureConfigurations(configurations, FeatureConfigurationSection.Runtime, ref dummy);
        }

        public static int SetLiveFeatureConfigurations(List<FeatureConfiguration> configurations, FeatureConfigurationSection section)
        {
            uint dummy = 0;
            return SetLiveFeatureConfigurations(configurations, section, ref dummy);
        }

        public static int SetLiveFeatureConfigurations(List<FeatureConfiguration> configurations, FeatureConfigurationSection section, ref uint changeStamp)
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

        public static int AddLiveFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            return NativeMethods.RtlSubscribeForFeatureUsageNotification(RtlDataHelpers.SerializeFeatureUsageSubscriptions(subscriptions), subscriptions.Count);
        }

        public static int RemoveLiveFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            return NativeMethods.RtlUnsubscribeFromFeatureUsageNotifications(RtlDataHelpers.SerializeFeatureUsageSubscriptions(subscriptions), subscriptions.Count);
        }

        public static int NotifyFeatureUsage(FeatureUsageReport report)
        {
            return NativeMethods.RtlNotifyFeatureUsage(RtlDataHelpers.SerializeFeatureUsageReport(report));
        }

        public static int SetBootFeatureConfigurationState(ref int state)
        {
            return NativeMethods.RtlSetSystemBootStatus(17, ref state, sizeof(int), IntPtr.Zero);
        }

        public static int GetBootFeatureConfigurationState(ref int state)
        {
            return NativeMethods.RtlGetSystemBootStatus(17, ref state, sizeof(int), IntPtr.Zero);
        }

        public static bool SetBootFeatureConfigurations(List<FeatureConfiguration> configurations)
        {
            try
            {
                foreach (var config in configurations)
                {
                    uint obfuscatedId = RtlDataHelpers.GetObfuscatedFeatureId(config.FeatureId);
                    var obfuscatedKey = $@"SYSTEM\CurrentControlSet\Control\FeatureManagement\Overrides\{config.Group}\{obfuscatedId}";
                    if (config.Action == FeatureConfigurationAction.Delete)
                        Registry.LocalMachine.DeleteSubKeyTree(obfuscatedKey, false);
                    else
                    {
                        using (var rKey = Registry.LocalMachine.CreateSubKey(obfuscatedKey))
                        {
                            if ((config.Action & FeatureConfigurationAction.UpdateEnabledState) == FeatureConfigurationAction.UpdateEnabledState)
                            {
                                rKey.SetValue("EnabledState", (int)config.EnabledState);
                                rKey.SetValue("EnabledStateOptions", config.EnabledStateOptions);
                            }
                            if ((config.Action & FeatureConfigurationAction.UpdateVariant) == FeatureConfigurationAction.UpdateVariant)
                            {
                                rKey.SetValue("Variant", config.Variant);
                                rKey.SetValue("VariantPayload", config.VariantPayload);
                                rKey.SetValue("VariantPayloadKind", config.VariantPayloadKind);
                            }
                        }
                    }
                }
                return true;
            } catch { return false; }
        }

        public static bool RemoveBootFeatureConfigurations(List<FeatureConfiguration> configurations)
        {
            try
            {
                foreach (var config in configurations)
                {
                    uint obfuscatedId = RtlDataHelpers.GetObfuscatedFeatureId(config.FeatureId);
                    Registry.LocalMachine.DeleteSubKeyTree($@"SYSTEM\CurrentControlSet\Control\FeatureManagement\Overrides\{config.Group}\{obfuscatedId}", false);
                }
                return true;
            }
            catch { return false; }
        }

        public static bool AddBootFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            try
            {
                foreach (var sub in subscriptions)
                {
                    uint obfuscatedId = RtlDataHelpers.GetObfuscatedFeatureId(sub.FeatureId);
                    using (var rKey = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Control\FeatureManagement\UsageSubscriptions\{obfuscatedId}\{{{Guid.NewGuid()}}}"))
                    {
                        rKey.SetValue("ReportingKind", (int)sub.ReportingKind);
                        rKey.SetValue("ReportingOptions", (int)sub.ReportingOptions);
                        rKey.SetValue("ReportingTarget", BitConverter.GetBytes(sub.ReportingTarget));
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public static bool RemoveBootFeatureUsageSubscriptions(List<FeatureUsageSubscription> subscriptions)
        {
            try
            {
                string[] bootSubs;
                using (var rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FeatureManagement\UsageSubscriptions"))
                    bootSubs = rKey.GetSubKeyNames();
                foreach (var sub in subscriptions)
                {
                    var obfuscatedKey = RtlDataHelpers.GetObfuscatedFeatureId(sub.FeatureId).ToString();
                    if (bootSubs.Contains(obfuscatedKey))
                    {
                        bool isEmpty = false;
                        obfuscatedKey = @"SYSTEM\CurrentControlSet\Control\FeatureManagement\UsageSubscriptions\" + obfuscatedKey;
                        using (var sKey = Registry.LocalMachine.OpenSubKey(obfuscatedKey, true))
                        {
                            foreach (var subGuid in sKey.GetSubKeyNames())
                            {
                                bool toRemove = false;
                                using (var gKey = sKey.OpenSubKey(subGuid))
                                {
                                    if ((int)gKey.GetValue("ReportingKind") == sub.ReportingKind &&
                                        (int)gKey.GetValue("ReportingOptions") == sub.ReportingOptions &&
                                        BitConverter.ToUInt64((byte[])gKey.GetValue("ReportingTarget"), 0) == sub.ReportingTarget)
                                        toRemove = true;
                                }
                                if (toRemove)
                                    sKey.DeleteSubKeyTree(subGuid, false);
                            }
                            isEmpty = sKey.SubKeyCount == 0;
                        }
                        if (isEmpty)
                            Registry.LocalMachine.DeleteSubKeyTree(obfuscatedKey, false);
                    }
                }
                return true;
            }
            catch { return false; }
        }
    }
}
