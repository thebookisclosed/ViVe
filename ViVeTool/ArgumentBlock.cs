/*
    ViVe - Windows feature configuration library
    Copyright (C) 2019-2022  @thebookisclosed

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
using System;
using System.Collections.Generic;

namespace Albacore.ViVeTool
{
    static class ArgumentBlock
    {
        internal static FeatureConfigurationTypeEx? Store;
        internal static List<uint> IdList;
        internal static FeatureConigurationProperties FeatureConigurationProperties;
        internal static SubscriptionProperties SubscriptionProperties;
#if SET_LKG_COMMAND
        internal static BSD_FEATURE_CONFIGURATION_STATE? LKGStatus;
#endif
        internal static string FileName;
        internal static bool ImportReplace;
        internal static bool HelpMode;

        internal static bool ShouldUseBothStores { get { return !Store.HasValue || Store.Value == FeatureConfigurationTypeEx.Both; } }

        internal static void Initialize(string[] args, ArgumentBlockFlags flags)
        {
            // Fast help path
            if (args.Length == 2 && (args[1] == "/?" || string.Compare(args[1], "/help", true) == 0))
            {
                HelpMode = true;
                return;
            }

            if (flags != 0 &&
                (flags.HasFlag(ArgumentBlockFlags.EnabledStateOptions) ||
                flags.HasFlag(ArgumentBlockFlags.Variant) ||
                flags.HasFlag(ArgumentBlockFlags.VariantPayloadKind) ||
                flags.HasFlag(ArgumentBlockFlags.VariantPayload) ||
                flags.HasFlag(ArgumentBlockFlags.Priority)))
                FeatureConigurationProperties = new FeatureConigurationProperties();

            if (flags != 0 &&
                (flags.HasFlag(ArgumentBlockFlags.ReportingKind) ||
                flags.HasFlag(ArgumentBlockFlags.ReportingOptions) ||
                flags.HasFlag(ArgumentBlockFlags.ReportingTarget)))
                SubscriptionProperties = new SubscriptionProperties();

            var bothStoresArgAllowed = flags.HasFlag(ArgumentBlockFlags.AllowBothStoresArgument);

            for (int i = 1; i < args.Length && flags != 0; i++)
            {
                var firstSc = args[i].IndexOf(':');
                //if (firstSc == -1) continue;
                var hasValue = firstSc != -1;
                var lower = args[i].ToLowerInvariant();
                var key = hasValue ? lower.Substring(0, firstSc) : lower;
                var value = hasValue ? args[i].Substring(firstSc + 1) : null;
                if (flags.HasFlag(ArgumentBlockFlags.Store) && key == "/store")
                {
                    if (Enum.TryParse(value, true, out FeatureConfigurationTypeEx parsedStore))
                    {
                        if (!bothStoresArgAllowed && parsedStore == FeatureConfigurationTypeEx.Both)
                        {
                            ConsoleEx.WriteErrorLine(Properties.Resources.InvalidEnumSpecInScenario, value, "Store");
                            HelpMode = true;
                            return;
                        }
                        Store = parsedStore;
                    }
                    else
                    {
                        ConsoleEx.WriteErrorLine(Properties.Resources.InvalidEnumSpec, value, "Store");
                        HelpMode = true;
                        return;
                    }
                    flags &= ~ArgumentBlockFlags.Store;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.IdList) && key == "/id")
                {
                    if (IdList == null)
                        IdList = new List<uint>();
                    foreach (var strId in value.Split(','))
                    {
                        if (TryParseDecHexUint(strId, out uint parsedId))
                            IdList.Add(parsedId);
                        else
                            Console.WriteLine("Unable to parse feature ID: {0}", strId);
                    }
                    flags &= ~ArgumentBlockFlags.IdList;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.NameList) && key == "/name")
                {
                    if (IdList == null)
                        IdList = new List<uint>();
                    var foundIds = FeatureNaming.FindIdsForNames(value.Split(','));
                    if (foundIds != null)
                        IdList.AddRange(foundIds);
                    flags &= ~ArgumentBlockFlags.NameList;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.EnabledStateOptions) && key == "/experiment")
                {
                    FeatureConigurationProperties.EnabledStateOptions = RTL_FEATURE_ENABLED_STATE_OPTIONS.WexpConfig;
                    flags &= ~ArgumentBlockFlags.EnabledStateOptions;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.Variant) && key == "/variant")
                {
                    if (TryParseDecHexUint(value, out uint parsedVariant))
                        FeatureConigurationProperties.Variant = parsedVariant;
                    flags &= ~ArgumentBlockFlags.Variant;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.VariantPayloadKind) && key == "/variantpayloadkind")
                {
                    if (!Enum.TryParse(value, true, out RTL_FEATURE_VARIANT_PAYLOAD_KIND parsedKind))
                    {
                        ConsoleEx.WriteErrorLine(Properties.Resources.InvalidEnumSpec, value, "Variant Payload Kind");
                        HelpMode = true;
                        return;
                    }
                    FeatureConigurationProperties.VariantPayloadKind = parsedKind;
                    flags &= ~ArgumentBlockFlags.VariantPayloadKind;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.VariantPayload) && key == "/variantpayload")
                {
                    if (TryParseDecHexUint(value, out uint parsedPayload))
                        FeatureConigurationProperties.VariantPayload = parsedPayload;
                    flags &= ~ArgumentBlockFlags.VariantPayload;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.Priority) && key == "/priority")
                {
                    if (!Enum.TryParse(value, true, out RTL_FEATURE_CONFIGURATION_PRIORITY parsedPriority))
                    {
                        ConsoleEx.WriteErrorLine(Properties.Resources.InvalidEnumSpec, value, "Priority");
                        HelpMode = true;
                        return;
                    }
                    FeatureConigurationProperties.Priority = parsedPriority;
                    flags &= ~ArgumentBlockFlags.Priority;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.ReportingKind) && key == "/reportingkind")
                {
                    if (TryParseDecHexUshort(value, out ushort parsedKind))
                        SubscriptionProperties.ReportingKind = parsedKind;
                    flags &= ~ArgumentBlockFlags.ReportingKind;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.ReportingOptions) && key == "/reportingoptions")
                {
                    if (TryParseDecHexUshort(value, out ushort parsedOptions))
                        SubscriptionProperties.ReportingOptions = parsedOptions;
                    flags &= ~ArgumentBlockFlags.ReportingOptions;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.ReportingTarget) && key == "/reportingtarget")
                {
                    if (ulong.TryParse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out ulong parsedTarget))
                        SubscriptionProperties.ReportingTarget = parsedTarget;
                    flags &= ~ArgumentBlockFlags.ReportingTarget;
                }
#if SET_LKG_COMMAND
                else if (flags.HasFlag(ArgumentBlockFlags.LKGStatus) && key == "/status")
                {
                    if (!Enum.TryParse(value, true, out BSD_FEATURE_CONFIGURATION_STATE parsedStatus))
                    {
                        ConsoleEx.WriteErrorLine(Properties.Resources.InvalidEnumSpec, value, "LKG Status");
                        HelpMode = true;
                        return;
                    }
                    LKGStatus = parsedStatus;
                    flags &= ~ArgumentBlockFlags.LKGStatus;
                }
#endif
                else if (flags.HasFlag(ArgumentBlockFlags.FileName) && key == "/filename")
                {
                    FileName = value;
                    flags &= ~ArgumentBlockFlags.FileName;
                }
                else if (flags.HasFlag(ArgumentBlockFlags.ImportReplace) && key == "/replace")
                {
                    ImportReplace = true;
                    flags &= ~ArgumentBlockFlags.ImportReplace;
                }
                else if (key == "/?" || string.Compare(key, "/help", true) == 0)
                {
                    HelpMode = true;
                    return;
                }
                else
                {
                    ConsoleEx.WriteWarnLine(Properties.Resources.UnrecognizedParameter, key);
                }
            }
        }

        private static bool TryParseDecHexUint(string input, out uint output)
        {
            bool success;
            if (input.StartsWith("0x"))
                success = uint.TryParse(input.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out output);
            else
                success = uint.TryParse(input, out output);
            return success;
        }

        private static bool TryParseDecHexUshort(string input, out ushort output)
        {
            bool success;
            if (input.StartsWith("0x"))
                success = ushort.TryParse(input.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out output);
            else
                success = ushort.TryParse(input, out output);
            return success;
        }
    }

    internal class FeatureConigurationProperties
    {
        internal RTL_FEATURE_ENABLED_STATE_OPTIONS EnabledStateOptions;
        internal uint Variant;
        internal RTL_FEATURE_VARIANT_PAYLOAD_KIND VariantPayloadKind;
        internal uint VariantPayload;
        internal RTL_FEATURE_CONFIGURATION_PRIORITY? Priority;
    }

    internal class SubscriptionProperties
    {
        internal ushort ReportingKind;
        internal ushort ReportingOptions;
        internal ulong ReportingTarget;
    }

    [Flags]
    enum ArgumentBlockFlags
    {
        Store = 1,
        IdList = 2,
        NameList = 4,
        EnabledStateOptions = 8,
        Variant = 16,
        VariantPayloadKind = 32,
        VariantPayload = 64,
        Priority = 128,
        ReportingKind = 256,
        ReportingOptions = 512,
        ReportingTarget = 1024,
        AllowBothStoresArgument = 2048,
#if SET_LKG_COMMAND
        LKGStatus = 4096,
#endif
        FileName = 8192,
        ImportReplace = 16384,
        Identifiers = IdList | NameList,
        FeatureConfigurationProperties = EnabledStateOptions | Variant | VariantPayloadKind | VariantPayload | Priority,
        SubscriptionProperties = ReportingKind | ReportingOptions | ReportingTarget,
        Export = Store | AllowBothStoresArgument | FileName
    }

    enum FeatureConfigurationTypeEx : uint
    {
        Boot = 0,
        Runtime = 1,
        Both = 2
    }
}
