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
            ref uint changeStamp,
            IntPtr buffer,
            ref int featureCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlQueryFeatureConfiguration(
            uint featureId,
            FeatureConfigurationSection sectionType,
            ref uint changeStamp,
            IntPtr buffer
            );

        [DllImport("ntdll.dll")]
        public static extern uint RtlQueryFeatureConfigurationChangeStamp();

        [DllImport("ntdll.dll")]
        public static extern int RtlQueryFeatureUsageNotificationSubscriptions(
            IntPtr buffer,
            ref int subscriptionCount
            );

        [DllImport("ntdll.dll")]
        public static extern int RtlSetFeatureConfigurations(
            ref uint changeStamp,
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
    }
}
