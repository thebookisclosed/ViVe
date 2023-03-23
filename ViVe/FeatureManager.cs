/*
    ViVe - Windows feature configuration library
    Copyright (C) 2019-2023  @thebookisclosed

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

using Albacore.ViVe.NativeEnums;
using Albacore.ViVe.NativeMethods;
using Albacore.ViVe.NativeStructs;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace Albacore.ViVe
{
    public static class FeatureManager
    {
        public unsafe static RTL_FEATURE_CONFIGURATION[] QueryAllFeatureConfigurations(RTL_FEATURE_CONFIGURATION_TYPE configurationType = RTL_FEATURE_CONFIGURATION_TYPE.Runtime)
        {
            return QueryAllFeatureConfigurations(configurationType, null);
        }

        public unsafe static RTL_FEATURE_CONFIGURATION[] QueryAllFeatureConfigurations(RTL_FEATURE_CONFIGURATION_TYPE configurationType, ref ulong changeStamp)
        {
            fixed (ulong* changeStampPtr = &changeStamp)
                return QueryAllFeatureConfigurations(configurationType, changeStampPtr);
        }

        public unsafe static RTL_FEATURE_CONFIGURATION[] QueryAllFeatureConfigurations(RTL_FEATURE_CONFIGURATION_TYPE configurationType, ulong* changeStamp)
        {
            Ntdll.RtlQueryAllFeatureConfigurations(configurationType, changeStamp, null, out int configCount);
            if (configCount == 0)
                return null;
            var allFeatureConfigs = new RTL_FEATURE_CONFIGURATION[configCount];
            int hRes;
            fixed (RTL_FEATURE_CONFIGURATION* configsPtr = allFeatureConfigs)
                hRes = Ntdll.RtlQueryAllFeatureConfigurations(configurationType, changeStamp, configsPtr, out configCount);
            if (hRes == 0)
                return allFeatureConfigs;
            else
                return null;
        }

        public static RTL_FEATURE_CONFIGURATION? QueryFeatureConfiguration(uint featureId, RTL_FEATURE_CONFIGURATION_TYPE configurationType = RTL_FEATURE_CONFIGURATION_TYPE.Runtime)
        {
            ulong dummy = 0;
            return QueryFeatureConfiguration(featureId, configurationType, ref dummy);
        }

        public static RTL_FEATURE_CONFIGURATION? QueryFeatureConfiguration(uint featureId, RTL_FEATURE_CONFIGURATION_TYPE configurationType, ref ulong changeStamp)
        {
            int hRes = Ntdll.RtlQueryFeatureConfiguration(featureId, configurationType, ref changeStamp, out RTL_FEATURE_CONFIGURATION config);
            if (hRes != 0)
                return null;
            return config;
        }

        public static ulong QueryFeatureConfigurationChangeStamp()
        {
            return Ntdll.RtlQueryFeatureConfigurationChangeStamp();
        }

        public static int SetFeatureConfigurations(RTL_FEATURE_CONFIGURATION_UPDATE[] updates, RTL_FEATURE_CONFIGURATION_TYPE configurationType = RTL_FEATURE_CONFIGURATION_TYPE.Runtime)
        {
            ulong dummy = 0;
            return SetFeatureConfigurations(updates, configurationType, ref dummy);
        }

        public static int SetFeatureConfigurations(RTL_FEATURE_CONFIGURATION_UPDATE[] updates, RTL_FEATURE_CONFIGURATION_TYPE configurationType, ref ulong previousChangeStamp)
        {
            foreach (var update in updates)
                if (update.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.ImageDefault ||
                    update.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.ImageOverride ||
                    update.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.Security)
                    throw new ArgumentException("ImageDefault (0), Security (9), and ImageOverride (15) priorities are protected and can't be written to.");
                else if (update.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.UserPolicy && !update.UserPolicyPriorityCompatible)
                    throw new ArgumentException("UserPolicy priority overrides do not support persisting properties other than EnabledState.");

            if (configurationType == RTL_FEATURE_CONFIGURATION_TYPE.Runtime)
                return Ntdll.RtlSetFeatureConfigurations(ref previousChangeStamp, RTL_FEATURE_CONFIGURATION_TYPE.Runtime, updates, updates.Length);
            else
                return SetFeatureConfigurationsInRegistry(updates, previousChangeStamp);
        }

        public static IntPtr RegisterFeatureConfigurationChangeNotification(FeatureConfigurationChangeCallback callback)
        {
            return RegisterFeatureConfigurationChangeNotification(callback, IntPtr.Zero);
        }

        public static IntPtr RegisterFeatureConfigurationChangeNotification(FeatureConfigurationChangeCallback callback, IntPtr context)
        {
            Ntdll.RtlRegisterFeatureConfigurationChangeNotification(callback, context, IntPtr.Zero, out IntPtr sub);
            return sub;
        }

        public static IntPtr RegisterFeatureConfigurationChangeNotification(FeatureConfigurationChangeCallback callback, IntPtr context, ref ulong waitForChangeStamp)
        {
            Ntdll.RtlRegisterFeatureConfigurationChangeNotification(callback, context, ref waitForChangeStamp, out IntPtr sub);
            return sub;
        }

        public static int UnregisterFeatureConfigurationChangeNotification(IntPtr notification)
        {
            return Ntdll.RtlUnregisterFeatureConfigurationChangeNotification(notification);
        }

        public unsafe static RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] QueryFeatureUsageSubscriptions()
        {
            Ntdll.RtlQueryFeatureUsageNotificationSubscriptions(null, out int subCount);
            if (subCount == 0)
                return null;
            var allSubscriptions = new RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[subCount];
            int hRes;
            fixed (RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS* subsPtr = allSubscriptions)
                hRes = Ntdll.RtlQueryFeatureUsageNotificationSubscriptions(subsPtr, out subCount);
            if (hRes == 0)
                return allSubscriptions;
            else
                return null;
        }

        public static int AddFeatureUsageSubscriptions(RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] subscriptions)
        {
            return Ntdll.RtlSubscribeForFeatureUsageNotification(subscriptions, subscriptions.Length);
        }

        public static int RemoveFeatureUsageSubscriptions(RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] subscriptions)
        {
            return Ntdll.RtlUnsubscribeFromFeatureUsageNotifications(subscriptions, subscriptions.Length);
        }

        public static int NotifyFeatureUsage(ref RTL_FEATURE_USAGE_REPORT report)
        {
            return Ntdll.RtlNotifyFeatureUsage(ref report);
        }

        private const int RtlBsdItemFeatureConfigurationState = 17;
        public static int SetBootFeatureConfigurationState(BSD_FEATURE_CONFIGURATION_STATE state)
        {
            var newState = (int)state;
            return Ntdll.RtlSetSystemBootStatus(RtlBsdItemFeatureConfigurationState, ref newState, sizeof(int), IntPtr.Zero);
        }

        public static int GetBootFeatureConfigurationState(out BSD_FEATURE_CONFIGURATION_STATE state)
        {
            var apiResult = Ntdll.RtlGetSystemBootStatus(RtlBsdItemFeatureConfigurationState, out int intState, sizeof(int), IntPtr.Zero);
            state = (BSD_FEATURE_CONFIGURATION_STATE)intState;
            return apiResult;
        }

        // The Last Known Good feature store sometimes gets corrupted by the OS due to a use-after-free bug in fcon.dll
        // We fix the corrupted store header to make sure that Windows won't completely wipe feature configurations in case it shifts into LKG mode
        // One feature is unfortunately lost to corruption in case this does happen, but it's still much better to lose one than to lose all
        public static bool FixLKGStore()
        {
            try
            {
                using (var rKey = Registry.LocalMachine.OpenSubKey(@"CurrentControlSet\Control\FeatureManagement\LastKnownGood"))
                {
                    var lkgBlob = (byte[])rKey.GetValue("LKGConfiguration");
                    // Header is a simple 32-bit zero
                    if (BitConverter.ToInt32(lkgBlob, 0) == 0)
                        return false;
                    var headerSize = sizeof(int);
                    var oneConfigSize = Marshal.SizeOf(typeof(RTL_FEATURE_CONFIGURATION));
                    var fixedBlob = new byte[lkgBlob.Length - oneConfigSize];
                    Array.Copy(lkgBlob, headerSize + oneConfigSize, fixedBlob, headerSize, fixedBlob.Length - headerSize);
                    rKey.SetValue("LKGConfiguration", fixedBlob);
                    return true;
                }
            } catch { return false; }
        }

        public static int InitializeBootStatusDataFile()
        {
            return Ntdll.RtlCreateBootStatusDataFile(null);
        }

        private static int SetFeatureConfigurationsInRegistry(RTL_FEATURE_CONFIGURATION_UPDATE[] updates, ulong previousStamp)
        {
            // Stamp behavior and return values are taken from equivalent kernel operations for runtime feature configuration
            if (previousStamp > 0)
            {
                var currentStamp = QueryFeatureConfigurationChangeStamp();
                if (previousStamp != currentStamp)
                    return unchecked((int)0xC0000001);
            }

            try
            {
                foreach (var update in updates)
                {
                    var isUserPolicy = update.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.UserPolicy;
                    var obfuscatedId = ObfuscationHelpers.ObfuscateFeatureId(update.FeatureId).ToString();
                    var overrideKey = isUserPolicy ?
                        @"SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides" :
                        $@"SYSTEM\CurrentControlSet\Control\FeatureManagement\Overrides\{(int)update.Priority}\{obfuscatedId}";
                    if (update.Operation == RTL_FEATURE_CONFIGURATION_OPERATION.ResetState)
                    {
                        if (isUserPolicy)
                        {
                            var rKey = Registry.LocalMachine.OpenSubKey(overrideKey);
                            if (rKey != null)
                            {
                                using (rKey)
                                    rKey.DeleteValue(obfuscatedId);
                            }
                        }
                        else
                            Registry.LocalMachine.DeleteSubKeyTree(overrideKey, false);
                    }
                    else
                    {
                        using (var rKey = Registry.LocalMachine.CreateSubKey(overrideKey))
                        {
                            if (update.Operation.HasFlag(RTL_FEATURE_CONFIGURATION_OPERATION.FeatureState))
                            {
                                if (isUserPolicy)
                                {
                                    rKey.SetValue(obfuscatedId, (int)update.EnabledState);
                                }
                                else
                                {
                                    rKey.SetValue("EnabledState", (int)update.EnabledState);
                                    rKey.SetValue("EnabledStateOptions", (int)update.EnabledStateOptions);
                                }
                            }
                            if (!isUserPolicy && update.Operation.HasFlag(RTL_FEATURE_CONFIGURATION_OPERATION.VariantState))
                            {
                                // Casting these is needed otherwise they end up getting written as strings
                                rKey.SetValue("Variant", (int)update.Variant);
                                rKey.SetValue("VariantPayload", (int)update.VariantPayload);
                                rKey.SetValue("VariantPayloadKind", (int)update.VariantPayloadKind);
                            }
                        }
                    }
                }
                return 0;
            } catch (Exception ex) { return ex.HResult; }
        }

        public static int AddFeatureUsageSubscriptionsToRegistry(RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] subscriptions)
        {
            try
            {
                foreach (var sub in subscriptions)
                {
                    uint obfuscatedId = ObfuscationHelpers.ObfuscateFeatureId(sub.FeatureId);
                    using (var rKey = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Control\FeatureManagement\UsageSubscriptions\{obfuscatedId}\{{{Guid.NewGuid()}}}"))
                    {
                        rKey.SetValue("ReportingKind", (int)sub.ReportingKind);
                        rKey.SetValue("ReportingOptions", (int)sub.ReportingOptions);
                        rKey.SetValue("ReportingTarget", BitConverter.GetBytes(sub.ReportingTarget));
                    }
                }
                return 0;
            }
            catch (Exception ex) { return ex.HResult; }
        }

        public static int RemoveFeatureUsageSubscriptionsFromRegistry(RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] subscriptions)
        {
            try
            {
                foreach (var sub in subscriptions)
                {
                    var obfuscatedKey = @"SYSTEM\CurrentControlSet\Control\FeatureManagement\UsageSubscriptions\" + 
                        ObfuscationHelpers.ObfuscateFeatureId(sub.FeatureId).ToString();
                    var sKey = Registry.LocalMachine.OpenSubKey(obfuscatedKey, true);
                    if (sKey != null)
                    {
                        var isEmpty = false;
                        using (sKey)
                        {
                            foreach (var subGuid in sKey.GetSubKeyNames())
                            {
                                var toRemove = false;
                                using (var gKey = sKey.OpenSubKey(subGuid))
                                {
                                    if ((int)gKey.GetValue("ReportingKind") == sub.ReportingKind &&
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
                return 0;
            }
            catch (Exception ex) { return ex.HResult; }
        }
    }
}
