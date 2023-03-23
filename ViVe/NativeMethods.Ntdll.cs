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
using Albacore.ViVe.NativeStructs;
using System;
using System.Runtime.InteropServices;

namespace Albacore.ViVe.NativeMethods
{
    public delegate void FeatureConfigurationChangeCallback(IntPtr Context);

    public static class Ntdll
    {
        // Kernel supports null pointer for change stamp ref, unlike single feature query
        [DllImport("ntdll.dll")]
        public unsafe static extern int RtlQueryAllFeatureConfigurations(
            RTL_FEATURE_CONFIGURATION_TYPE featureConfigurationType,
            ulong* changeStamp,
            RTL_FEATURE_CONFIGURATION* featureConfigurations,
            out int featureConfigurationCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlQueryFeatureConfiguration(
            uint featureId,
            RTL_FEATURE_CONFIGURATION_TYPE featureConfigurationType,
            ref ulong changeStamp,
            out RTL_FEATURE_CONFIGURATION featureConfiguration
            );

        [DllImport("ntdll.dll")]
        public static extern ulong RtlQueryFeatureConfigurationChangeStamp();

        [DllImport("ntdll.dll")]
        public unsafe static extern int RtlQueryFeatureUsageNotificationSubscriptions(
            RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS* subscriptions,
            out int subscriptionCount
            );

        // Kernel treats pointer to 0 the same as a null pointer, no alternate signature required
        [DllImport("ntdll.dll")]
        public static extern int RtlSetFeatureConfigurations(
            ref ulong previousChangeStamp,
            RTL_FEATURE_CONFIGURATION_TYPE featureConfigurationType,
            RTL_FEATURE_CONFIGURATION_UPDATE[] featureConfigurations,
            int featureConfigurationCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlRegisterFeatureConfigurationChangeNotification(
            FeatureConfigurationChangeCallback callback,
            IntPtr context,
            IntPtr waitForChangeStamp,
            out IntPtr subscription
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlRegisterFeatureConfigurationChangeNotification(
            FeatureConfigurationChangeCallback callback,
            IntPtr context,
            ref ulong waitForChangeStamp,
            out IntPtr subscription
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlUnregisterFeatureConfigurationChangeNotification(
            IntPtr subscription
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlSubscribeForFeatureUsageNotification(
            RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] subscriptions,
            int subscriptionCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlUnsubscribeFromFeatureUsageNotifications(
            RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[] subscriptions,
            int subscriptionCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlNotifyFeatureUsage(
            ref RTL_FEATURE_USAGE_REPORT report
            );

        // BSD Items can have varying sizes, int signature is enough for our needs though
        [DllImport("ntdll.dll")]
        public static extern int RtlSetSystemBootStatus(
            int bsdItemType,
            ref int data,
            int dataLength,
            IntPtr returnLength
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlGetSystemBootStatus(
            int bsdItemType,
            out int data,
            int dataLength,
            IntPtr returnLength
            );

        [DllImport("ntdll.dll", CharSet = CharSet.Unicode)]
        public static extern int RtlCreateBootStatusDataFile(string bootStatusPath);
    }
}
