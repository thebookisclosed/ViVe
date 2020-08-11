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

using System;
using System.Runtime.InteropServices;

namespace Albacore.ViVe
{
    public delegate void FeatureConfigurationChangeCallback(IntPtr Context);

    public static class NativeMethods
    {
        [DllImport("ntdll.dll")]
        public static extern int RtlQueryAllFeatureConfigurations(
            FeatureConfigurationSection sectionType,
            ref ulong changeStamp,
            IntPtr buffer,
            ref int featureCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlQueryFeatureConfiguration(
            uint featureId,
            FeatureConfigurationSection sectionType,
            ref ulong changeStamp,
            IntPtr buffer
            );

        [DllImport("ntdll.dll")]
        public static extern ulong RtlQueryFeatureConfigurationChangeStamp();

        [DllImport("ntdll.dll")]
        public static extern int RtlQueryFeatureUsageNotificationSubscriptions(
            IntPtr buffer,
            ref int subscriptionCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlSetFeatureConfigurations(
            ref ulong changeStamp,
            FeatureConfigurationSection sectionType,
            byte[] buffer,
            int featureCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlRegisterFeatureConfigurationChangeNotification(
            FeatureConfigurationChangeCallback callback,
            IntPtr context,
            IntPtr unknown,
            out IntPtr subscription
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlUnregisterFeatureConfigurationChangeNotification(
            IntPtr subscription
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlSubscribeForFeatureUsageNotification(
            byte[] buffer,
            int subscriptionCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlUnsubscribeFromFeatureUsageNotifications(
            byte[] buffer,
            int subscriptionCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlNotifyFeatureUsage(
            byte[] buffer
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlSetSystemBootStatus(
            int infoClass,
            ref int state,
            int stateSize,
            IntPtr output
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlGetSystemBootStatus(
            int infoClass,
            ref int state,
            int stateSize,
            IntPtr output
            );
    }
}
