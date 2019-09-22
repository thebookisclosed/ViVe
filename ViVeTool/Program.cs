/*
    ViVeTool - Vibranium feature configuration tool
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
using System.Collections.Generic;
using Albacore.ViVe;

namespace Albacore.ViVeTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ViVeTool v0.1.1 - Vibranium feature configuration tool\n");
            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }
            if (Environment.OSVersion.Version.Build < 18963)
            {
                Console.WriteLine("Windows 10 build 18963 or newer is required for this program to function");
                return;
            }
            ProcessArgs(args);
        }

        static void ProcessArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
                args[i] = args[i].ToLower();

            #region QueryConfig
            if (args[0] == "queryconfig")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Syntax:   queryconfig <all | numeric id> [runtime | boot]\n");
                    Console.WriteLine("Examples: queryconfig all\n          queryconfig 123456 runtime\n");
                    Console.WriteLine("If no section or an invalid section is specified, 'runtime' will be used");
                    return;
                }
                FeatureConfigurationSection sectionToUse = FeatureConfigurationSection.Runtime;
                if (args.Length > 2 && args[2] == "boot")
                    sectionToUse = FeatureConfigurationSection.Boot;
                if (args[1] == "all")
                {
                    var retrievedConfigs = RtlFeatureManager.QueryAllFeatureConfigurations(sectionToUse);
                    if (retrievedConfigs != null)
                    {
                        foreach (var config in retrievedConfigs)
                        {
                            PrintFeatureConfig(config);
                            Console.WriteLine();
                        }
                    }
                    else
                        Console.WriteLine("Failed to query feature configurations");
                }
                else
                {
                    if (!TryParseDecHexUint(args[1], out uint featureId))
                    {
                        Console.WriteLine("Couldn't parse feature ID");
                        return;
                    }
                    var config = RtlFeatureManager.QueryFeatureConfiguration(featureId, sectionToUse);
                    if (config != null)
                        PrintFeatureConfig(config);
                    else
                        Console.WriteLine("Failed to query feature configuration, the specified ID may not exist in the store");
                }
            }
            #endregion
            #region QuerySubs
            else if (args[0] == "querysubs")
            {
                var retrievedSubs = RtlFeatureManager.QueryFeatureUsageSubscriptions();
                if (retrievedSubs != null)
                {
                    foreach (var sub in retrievedSubs)
                    {
                        PrintSubscription(sub);
                        Console.WriteLine();
                    }
                }
            }
            #endregion
            #region ChangeStamp
            else if (args[0] == "changestamp")
            {
                Console.WriteLine("Change stamp: {0}", RtlFeatureManager.QueryFeatureConfigurationChangeStamp());
            }
            #endregion
            #region AddConfig
            else if (args[0] == "addconfig")
            {
                ProcessSetConfig(args, FeatureConfigurationAction.UpdateAll);
            }
            #endregion
            #region DelConfig
            else if (args[0] == "delconfig")
            {
                ProcessSetConfig(args, FeatureConfigurationAction.Delete);
            }
            #endregion
            #region AddSub
            else if (args[0] == "addsub")
            {
                ProcessSetSubs(args, false);
            }
            #endregion
            #region DelSub
            else if (args[0] == "delsub")
            {
                ProcessSetSubs(args, true);
            }
            #endregion
            #region NotifyUsage
            else if (args[0] == "notifyusage")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Syntax:   notifyusage <numeric id> <reporting kind> <reporting options>\n");
                    Console.WriteLine("Examples: notifyusage 123465 2 1");
                    return;
                }
                if (!TryParseDecHexUint(args[1], out uint featureId))
                {
                    Console.WriteLine("Couldn't parse feature ID");
                    return;
                }
                if (!TryParseDecHexUshort(args[2], out ushort reportingKind))
                {
                    Console.WriteLine("Couldn't parse reporting kind");
                    return;
                }
                if (!TryParseDecHexUshort(args[3], out ushort reportingOptions))
                {
                    Console.WriteLine("Couldn't parse reporting options");
                    return;
                }
                int result = RtlFeatureManager.NotifyFeatureUsage(new FeatureUsageReport() {
                        FeatureId = featureId,
                        ReportingKind = reportingKind,
                        ReportingOptions = reportingOptions
                    });
                if (result != 0)
                    Console.WriteLine("An error occurred while firing the notification (hr=0x{0:x8}), no such subscription may exist", result);
                else
                    Console.WriteLine("Successfully fired the feature usage notification");
            }
            #endregion
        }

        static void ProcessSetSubs(string[] args, bool del)
        {
            if (args.Length < 5)
            {
                string helpPrefix = del ? "del" : "add";
                Console.WriteLine("Syntax:   {0}sub <numeric id> <reporting kind> <reporting options> <reporting target>\n", helpPrefix);
                Console.WriteLine("Reporting targets are WNF state names\n");
                Console.WriteLine("Examples: {0}sub 123456 2 1 0d83063ea3bdf875", helpPrefix);
                return;
            }
            if (!TryParseDecHexUint(args[1], out uint featureId))
            {
                Console.WriteLine("Couldn't parse feature ID");
                return;
            }
            if (!TryParseDecHexUshort(args[2], out ushort reportingKind))
            {
                Console.WriteLine("Couldn't parse reporting kind");
                return;
            }
            if (!TryParseDecHexUshort(args[3], out ushort reportingOptions))
            {
                Console.WriteLine("Couldn't parse reporting options");
                return;
            }
            if (!ulong.TryParse(args[4], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out ulong reportingTarget))
            {
                Console.WriteLine("Couldn't parse reporting target");
                return;
            }
            int result;
            if (del)
                result = RtlFeatureManager.RemoveFeatureUsageSubscriptions(new List<FeatureUsageSubscription>() {
                    new FeatureUsageSubscription() {
                        FeatureId = featureId,
                        ReportingKind = reportingKind,
                        ReportingOptions = reportingOptions,
                        ReportingTarget = reportingTarget
                    } });
            else
                result = RtlFeatureManager.AddFeatureUsageSubscriptions(new List<FeatureUsageSubscription>() {
                    new FeatureUsageSubscription() {
                        FeatureId = featureId,
                        ReportingKind = reportingKind,
                        ReportingOptions = reportingOptions,
                        ReportingTarget = reportingTarget
                    } });
            if (result != 0)
                Console.WriteLine("An error occurred while setting the feature usage subscription (hr=0x{0:x8})", result);
            else
                Console.WriteLine("Successfully set feature usage subscription");
        }

        static void ProcessSetConfig(string[] args, FeatureConfigurationAction actionToUse)
        {
            if (args.Length < 4)
            {
                string helpPrefix = actionToUse == FeatureConfigurationAction.Delete ? "del" : "add";
                Console.WriteLine("Syntax:   {0}config <runtime | boot> <numeric id> <enabled state>\n          [enabled state options] [variant] [variant payload kind] [variant payload] [group]\n", helpPrefix);
                Console.WriteLine("Enabled state types: 0 = Default, 1 = Disabled, 2 = Enabled\n");
                Console.WriteLine("Examples: {0}config runtime 123456 2\n          {0}config runtime 456789 2 1 4 1 0 0", helpPrefix);
                return;
            }
            FeatureConfigurationSection sectionToUse;
            if (args[1] == "boot")
                sectionToUse = FeatureConfigurationSection.Boot;
            else if (args[1] == "runtime")
                sectionToUse = FeatureConfigurationSection.Runtime;
            else
            {
                Console.WriteLine("Feature configuration section must be 'boot' or 'runtime'");
                return;
            }

            if (!TryParseDecHexUint(args[2], out uint featureId))
            {
                Console.WriteLine("Couldn't parse feature ID");
                return;
            }
            if (!TryParseDecHexInt(args[3], out int enabledState))
            {
                Console.WriteLine("Couldn't parse enabled state");
                return;
            }
            int enabledStateOptions = 1;
            int variant = 0;
            int variantPayloadKind = 0;
            int variantPayload = 0;
            int group = 4;
            if (args.Length > 4 && !TryParseDecHexInt(args[4], out enabledStateOptions))
            {
                Console.WriteLine("Couldn't parse enabled state options");
                return;
            }
            if (args.Length > 5 && !TryParseDecHexInt(args[5], out variant))
            {
                Console.WriteLine("Couldn't parse variant");
                return;
            }
            if (args.Length > 6 && !TryParseDecHexInt(args[6], out variantPayloadKind))
            {
                Console.WriteLine("Couldn't parse variant payload kind");
                return;
            }
            if (args.Length > 7 && !TryParseDecHexInt(args[7], out variantPayload))
            {
                Console.WriteLine("Couldn't parse variant payload");
                return;
            }
            if (args.Length > 8 && !TryParseDecHexInt(args[8], out group))
            {
                Console.WriteLine("Couldn't parse group");
                return;
            }
            int result = RtlFeatureManager.SetFeatureConfigurations(new List<FeatureConfiguration>() {
                    new FeatureConfiguration() {
                        FeatureId = featureId,
                        EnabledState = (FeatureEnabledState)enabledState,
                        EnabledStateOptions = enabledStateOptions,
                        Group = group,
                        Variant = variant,
                        VariantPayloadKind = variantPayloadKind,
                        VariantPayload = variantPayload,
                        Action = actionToUse
                    } }, sectionToUse);
            if (result != 0)
                Console.WriteLine("An error occurred while setting the feature configuration (hr=0x{0:x8})", result);
            else
                Console.WriteLine("Successfully set feature configuration");
        }

        static bool TryParseDecHexUshort(string input, out ushort output)
        {
            bool success;
            if (input.StartsWith("0x"))
                success = ushort.TryParse(input.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out output);
            else
                success = ushort.TryParse(input, out output);
            return success;
        }

        static bool TryParseDecHexUint(string input, out uint output)
        {
            bool success;
            if (input.StartsWith("0x"))
                success = uint.TryParse(input.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out output);
            else
                success = uint.TryParse(input, out output);
            return success;
        }

        static bool TryParseDecHexInt(string input, out int output)
        {
            bool success;
            if (input.StartsWith("0x"))
                success = int.TryParse(input, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out output);
            else
                success = int.TryParse(input, out output);
            return success;
        }

        static void PrintHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  queryconfig\tLists existing feature configuration(s)");
            Console.WriteLine("  querysubs\tLists existing feature usage notification subscriptions");
            Console.WriteLine("  changestamp\tPrints the current feature store change stamp");
            Console.WriteLine("  addconfig\tAdds a feature configuration");
            Console.WriteLine("  delconfig\tDeletes a feature configuration");
            Console.WriteLine("  addsub\tAdds a feature usage subscription notification");
            Console.WriteLine("  delsub\tDeletes a feature usage subscription notification");
            Console.WriteLine("  notifyusage\tFires a feature usage notification");
        }

        static void PrintFeatureConfig(FeatureConfiguration config)
        {
            Console.WriteLine("[0x{0:X8} / {0}]", config.FeatureId);
            Console.WriteLine("Group: {0}", config.Group);
            Console.WriteLine("EnabledState: {0} ({1})", config.EnabledState, (int)config.EnabledState);
            Console.WriteLine("EnabledStateOptions: {0}", config.EnabledStateOptions);
            Console.WriteLine("Variant: {0}", config.Variant);
            Console.WriteLine("VariantPayloadKind: {0}", config.VariantPayloadKind);
            Console.WriteLine("VariantPayload: {0:x}", config.VariantPayload);
        }

        static void PrintSubscription(FeatureUsageSubscription sub)
        {
            Console.WriteLine("[0x{0:X8} / {0}]", sub.FeatureId);
            Console.WriteLine("ReportingKind: {0}", sub.ReportingKind);
            Console.WriteLine("ReportingOptions: {0}", sub.ReportingOptions);
            Console.WriteLine("ReportingTarget: {0:x16}", sub.ReportingTarget);
        }
    }
}
