/*
    ViVeTool - Windows feature configuration tool
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Albacore.ViVe;
using Albacore.ViVe.NativeEnums;
using Albacore.ViVe.NativeStructs;

namespace Albacore.ViVeTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Properties.Resources.Branding);
            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }
            if (Environment.OSVersion.Version.Build < 18963)
            {
                // An attempt at stopping people from mistaking 18363 for 18963 by showing them both target & current numbers
                Console.WriteLine(Properties.Resources.IncompatibleBuild, Environment.OSVersion.Version.Build);
                return;
            }
            ProcessArgs(args);
            if (Debugger.IsAttached)
                Console.ReadKey();
        }

        static void PrintHelp()
        {
            Console.WriteLine(Properties.Resources.Help_Commands);
        }

        static void ProcessArgs(string[] args)
        {
            var mainCmd = args[0].ToLowerInvariant();
            switch (mainCmd)
            {
                #region Current commands
                case "/query":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Store | ArgumentBlockFlags.Identifiers);
                    HandleQuery();
                    break;
                case "/enable":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Store | ArgumentBlockFlags.Identifiers | ArgumentBlockFlags.FeatureConfigurationProperties | ArgumentBlockFlags.AllowBothStoresArgument);
                    HandleSet(RTL_FEATURE_ENABLED_STATE.Enabled);
                    break;
                case "/disable":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Store | ArgumentBlockFlags.Identifiers | ArgumentBlockFlags.FeatureConfigurationProperties | ArgumentBlockFlags.AllowBothStoresArgument);
                    HandleSet(RTL_FEATURE_ENABLED_STATE.Disabled);
                    break;
                case "/reset":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Store | ArgumentBlockFlags.Identifiers | ArgumentBlockFlags.Priority | ArgumentBlockFlags.AllowBothStoresArgument);
                    HandleReset();
                    break;
                case "/fullreset":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Store | ArgumentBlockFlags.AllowBothStoresArgument);
                    HandleFullReset();
                    break;
                case "/changestamp":
                    HandleChangeStamp();
                    break;
                case "/querysubs":
                    HandleQuerySubs();
                    break;
                case "/addsub":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Identifiers | ArgumentBlockFlags.SubscriptionProperties);
                    HandleSetSubs(false);
                    break;
                case "/delsub":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Identifiers | ArgumentBlockFlags.ReportingKind | ArgumentBlockFlags.ReportingTarget);
                    HandleSetSubs(true);
                    break;
                case "/notifyusage":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Identifiers | ArgumentBlockFlags.ReportingKind | ArgumentBlockFlags.ReportingOptions);
                    HandleNotifyUsage();
                    break;
                case "/lkgstatus":
                    ArgumentBlock.Initialize(args, 0);
                    HandleLKGStatus();
                    break;
#if SET_LKG_COMMAND
                case "/setlkg":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.LKGStatus);
                    HandleSetLKG();
                    break;
#endif
                case "/export":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Export);
                    HandleExport();
                    break;
                case "/import":
                    ArgumentBlock.Initialize(args, ArgumentBlockFlags.Export | ArgumentBlockFlags.ImportReplace);
                    HandleImport();
                    break;
                case "/fixlkg":
                    ArgumentBlock.Initialize(args, 0);
                    HandleFixLKG();
                    break;
                case "/appupdate":
                    HandleAppUpdate();
                    break;
                case "/dictupdate":
                    HandleDictUpdate();
                    break;
                case "/?":
                case "/help":
                    PrintHelp();
                    break;
                #endregion
                #region Migration tips
                case "queryconfig":
                    CommandMigrationInfoTip(mainCmd, "/query");
                    break;
                case "addconfig":
                    CommandMigrationInfoTip(mainCmd, "/enable' & '/disable");
                    break;
                case "delconfig":
                    CommandMigrationInfoTip(mainCmd, "/reset");
                    break;
                case "querysubs":
                    CommandMigrationInfoTip(mainCmd, "/querysubs");
                    break;
                case "addsub":
                    CommandMigrationInfoTip(mainCmd, "/addsub");
                    break;
                case "delsub":
                    CommandMigrationInfoTip(mainCmd, "/delsub");
                    break;
                case "notifyusage":
                    CommandMigrationInfoTip(mainCmd, "/notifyusage");
                    break;
                case "changestamp":
                    CommandMigrationInfoTip(mainCmd, "/changestamp");
                    break;
                #endregion
                default:
                    ConsoleEx.WriteWarnLine(Properties.Resources.UnrecognizedCommand, mainCmd);
                    PrintHelp();
                    break;
            }
        }

        #region Main handlers
        static void HandleQuery()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_Query);
                return;
            }

            var storeToUse = ArgumentBlock.Store.HasValue ? (RTL_FEATURE_CONFIGURATION_TYPE)ArgumentBlock.Store.Value : RTL_FEATURE_CONFIGURATION_TYPE.Runtime;
            if (ArgumentBlock.IdList == null || ArgumentBlock.IdList.Count == 0)
            {
                var retrievedConfigs = FeatureManager.QueryAllFeatureConfigurations(storeToUse);
                if (retrievedConfigs != null)
                {
                    var namesAll = FeatureNaming.FindNamesForFeatures(retrievedConfigs.Select(x => x.FeatureId));
                    foreach (var config in retrievedConfigs)
                    {
                        string name = null;
                        try { name = namesAll[config.FeatureId]; } catch { }
                        PrintFeatureConfig(config, name);
                    }
                }
                else
                    ConsoleEx.WriteErrorLine(Properties.Resources.QueryFailed);
                return;
            }

            var namesSpecific = FeatureNaming.FindNamesForFeatures(ArgumentBlock.IdList);
            foreach (var id in ArgumentBlock.IdList)
            {
                var config = FeatureManager.QueryFeatureConfiguration(id, storeToUse);
                if (config != null)
                {
                    string name = null;
                    try { name = namesSpecific[id]; } catch { }
                    PrintFeatureConfig(config.Value, name);
                }
                else
                {
                    ConsoleEx.WriteErrorLine(Properties.Resources.SingleQueryFailed, id, storeToUse);
                    if (storeToUse == RTL_FEATURE_CONFIGURATION_TYPE.Boot)
                        ConsoleEx.WriteWarnLine(Properties.Resources.BootStoreRebootTip);
                }
            }
        }

        static void HandleSet(RTL_FEATURE_ENABLED_STATE state)
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_Set, state == RTL_FEATURE_ENABLED_STATE.Enabled ? "/enable" : "/disable");
                return;
            }

            if (ArgumentBlock.IdList == null || ArgumentBlock.IdList.Count == 0)
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoFeaturesSpecified);
                return;
            }

            var fcp = ArgumentBlock.FeatureConigurationProperties;
            var updates = new RTL_FEATURE_CONFIGURATION_UPDATE[ArgumentBlock.IdList.Count];
            for (int i = 0; i < updates.Length; i++)
            {
                updates[i] = new RTL_FEATURE_CONFIGURATION_UPDATE()
                {
                    FeatureId = ArgumentBlock.IdList[i],
                    EnabledState = state,
                    EnabledStateOptions = fcp.EnabledStateOptions,
                    Priority = fcp.Priority ?? RTL_FEATURE_CONFIGURATION_PRIORITY.Service,
                    Variant = fcp.Variant,
                    VariantPayloadKind = fcp.VariantPayloadKind,
                    VariantPayload = fcp.VariantPayload,
                    Operation = RTL_FEATURE_CONFIGURATION_OPERATION.FeatureState | RTL_FEATURE_CONFIGURATION_OPERATION.VariantState
                };
            }

            FinalizeSet(updates, false);
        }

        static void HandleReset()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_Reset);
                return;
            }

            if (ArgumentBlock.IdList == null || ArgumentBlock.IdList.Count == 0)
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoFeaturesSpecified);
                return;
            }

            RTL_FEATURE_CONFIGURATION_UPDATE[] updates;
            var fcp = ArgumentBlock.FeatureConigurationProperties;
            if (fcp.Priority.HasValue)
            {
                var priority = ArgumentBlock.FeatureConigurationProperties.Priority.Value;
                updates = new RTL_FEATURE_CONFIGURATION_UPDATE[ArgumentBlock.IdList.Count];
                for (int i = 0; i < updates.Length; i++)
                {
                    updates[i] = new RTL_FEATURE_CONFIGURATION_UPDATE()
                    {
                        FeatureId = ArgumentBlock.IdList[i],
                        Priority = priority,
                        Operation = RTL_FEATURE_CONFIGURATION_OPERATION.ResetState
                    };
                }
            }
            else
                updates = FindResettables(false);

            FinalizeSet(updates, true);
        }

        static void HandleFullReset()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_FullReset);
                return;
            }

            var userConsent = ConsoleEx.UserQuestion(Properties.Resources.FullResetPrompt);
            if (!userConsent)
            {
                Console.WriteLine(Properties.Resources.FullResetCanceled);
                return;
            }

            var updates = FindResettables(true);

            FinalizeSet(updates, true);
        }

        static void HandleChangeStamp()
        {
            Console.WriteLine(Properties.Resources.ChangestampDisplay, FeatureManager.QueryFeatureConfigurationChangeStamp());
        }

        static void HandleQuerySubs()
        {
            var retrievedSubs = FeatureManager.QueryFeatureUsageSubscriptions();
            if (retrievedSubs != null)
            {
                var names = FeatureNaming.FindNamesForFeatures(retrievedSubs.Select(x => x.FeatureId));
                foreach (var sub in retrievedSubs)
                {
                    string name = null;
                    try { name = names[sub.FeatureId]; } catch { }
                    PrintSubscription(sub, name);
                }
            }
            else
                ConsoleEx.WriteErrorLine(Properties.Resources.QuerySubsFailed);
        }

        // No individual Store management is supported due to queries effectively being Runtime only
        static void HandleSetSubs(bool delete)
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_SetSubs,
                    delete ? "/delsub" : "/addsub",
                    delete ? "" : " [/reportingoptions:<0-65535>]",
                    delete ? Properties.Resources.Help_SetSubs_Delete : Properties.Resources.Help_SetSubs_Add);
                return;
            }

            var sp = ArgumentBlock.SubscriptionProperties;
            if (ArgumentBlock.IdList == null || ArgumentBlock.IdList.Count == 0)
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoFeaturesSpecified);
                return;
            }
            else if (sp.ReportingTarget == 0)
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoReportingTargetSpecified);
                return;
            }

            var subs = new RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS[ArgumentBlock.IdList.Count];
            for (int i = 0; i < subs.Length; i++)
            {
                subs[i] = new RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS()
                {
                    FeatureId = ArgumentBlock.IdList[i],
                    ReportingKind = sp.ReportingKind,
                    ReportingOptions = sp.ReportingOptions,
                    ReportingTarget = sp.ReportingTarget
                };
            }
            int result;
            if (delete)
                result = FeatureManager.RemoveFeatureUsageSubscriptions(subs);
            else
                result = FeatureManager.AddFeatureUsageSubscriptions(subs);
            if (result != 0)
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.SetSubsRuntimeFailed,
                    GetHumanErrorDescription(result));
                return;
            }
            if (delete)
                result = FeatureManager.RemoveFeatureUsageSubscriptionsFromRegistry(subs);
            else
                result = FeatureManager.AddFeatureUsageSubscriptionsToRegistry(subs);
            if (result != 0)
                ConsoleEx.WriteErrorLine(Properties.Resources.SetSubsBootFailed,
                    GetHumanErrorDescription(result, true));
            else
                Console.WriteLine(Properties.Resources.SetSubsSuccess);
        }

        static void HandleNotifyUsage()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_NotifyUsage);
                return;
            }

            if (ArgumentBlock.IdList == null || ArgumentBlock.IdList.Count == 0)
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoFeaturesSpecified);
                return;
            }

            var sp = ArgumentBlock.SubscriptionProperties;
            foreach (var id in ArgumentBlock.IdList)
            {
                var report = new RTL_FEATURE_USAGE_REPORT()
                {
                    FeatureId = id,
                    ReportingKind = sp.ReportingKind,
                    ReportingOptions = sp.ReportingOptions
                };
                int result = FeatureManager.NotifyFeatureUsage(ref report);
                if (result != 0)
                    ConsoleEx.WriteErrorLine(Properties.Resources.NotifyUsageFailed,
                        id,
                        GetHumanErrorDescription(result));
                else
                    Console.WriteLine(Properties.Resources.NotifyUsageSuccess, id);
            }
        }

        static void HandleLKGStatus()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_LKGStatus);
                return;
            }

            var result = FeatureManager.GetBootFeatureConfigurationState(out BSD_FEATURE_CONFIGURATION_STATE state);
            if (result != 0)
                ConsoleEx.WriteErrorLine(Properties.Resources.LKGQueryFailed,
                    GetHumanErrorDescription(result));
            else
                Console.WriteLine(Properties.Resources.LKGStatusDisplay, state);
        }

#if SET_LKG_COMMAND
        static void HandleSetLKG()
        {
            UpdateLKGStatus(ArgumentBlock.LKGStatus.Value);
        }
#endif

        static void HandleExport()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_Export);
                return;
            }

            if (string.IsNullOrEmpty(ArgumentBlock.FileName))
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoFileNameSpecified);
                return;
            }

            var useBothStores = ArgumentBlock.ShouldUseBothStores;
            RTL_FEATURE_CONFIGURATION[] runtimeFeatures = null;
            RTL_FEATURE_CONFIGURATION[] bootFeatures = null;
            if (useBothStores || ArgumentBlock.Store.Value == FeatureConfigurationTypeEx.Runtime)
                runtimeFeatures = FeatureManager.QueryAllFeatureConfigurations(RTL_FEATURE_CONFIGURATION_TYPE.Runtime);
            if (useBothStores || ArgumentBlock.Store.Value == FeatureConfigurationTypeEx.Boot)
                bootFeatures = FeatureManager.QueryAllFeatureConfigurations(RTL_FEATURE_CONFIGURATION_TYPE.Boot);

            using (var fs = new FileStream(ArgumentBlock.FileName, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                SerializeConfigsToStream(bw, runtimeFeatures);
                SerializeConfigsToStream(bw, bootFeatures);
            }

            Console.WriteLine(Properties.Resources.ExportSuccess, runtimeFeatures?.Length ?? 0, bootFeatures?.Length ?? 0, ArgumentBlock.FileName);
        }

        static void HandleImport()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_Import);
                return;
            }

            if (string.IsNullOrEmpty(ArgumentBlock.FileName))
            {
                ConsoleEx.WriteErrorLine(Properties.Resources.NoFileNameSpecified);
                return;
            }

            List<RTL_FEATURE_CONFIGURATION> runtimeFeatures, bootFeatures;
            using (var fs = new FileStream(ArgumentBlock.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var br = new BinaryReader(fs))
            {
                runtimeFeatures = DeserializeConfigsFromStream(br);
                bootFeatures = DeserializeConfigsFromStream(br);
            }

            Console.WriteLine(Properties.Resources.ImportBreakdown, runtimeFeatures.Count, bootFeatures.Count, ArgumentBlock.FileName);

            if (ArgumentBlock.ImportReplace)
                HandleFullReset();

            var useBothStores = ArgumentBlock.ShouldUseBothStores;
            if ((useBothStores || ArgumentBlock.Store.Value == FeatureConfigurationTypeEx.Runtime) && runtimeFeatures.Count > 0)
            {
                Console.WriteLine(Properties.Resources.ImportProcessing, runtimeFeatures.Count, FeatureConfigurationTypeEx.Runtime);
                var updates = ConvertConfigsToUpdates(runtimeFeatures);
                var prevValue = ArgumentBlock.Store;
                ArgumentBlock.Store = FeatureConfigurationTypeEx.Runtime;
                FinalizeSet(updates, false);
                ArgumentBlock.Store = prevValue;
            }
            if ((useBothStores || ArgumentBlock.Store.Value == FeatureConfigurationTypeEx.Boot) && bootFeatures.Count > 0)
            {
                Console.WriteLine(Properties.Resources.ImportProcessing, runtimeFeatures.Count, FeatureConfigurationTypeEx.Boot);
                var updates = ConvertConfigsToUpdates(bootFeatures);
                var prevValue = ArgumentBlock.Store;
                ArgumentBlock.Store = FeatureConfigurationTypeEx.Boot;
                FinalizeSet(updates, false);
                ArgumentBlock.Store = prevValue;

                Console.WriteLine(Properties.Resources.RebootRecommended);
            }
        }

        static void HandleFixLKG()
        {
            if (ArgumentBlock.HelpMode)
            {
                Console.WriteLine(Properties.Resources.Help_FixLKG);
                return;
            }

            var fixPerformed = FeatureManager.FixLKGStore();
            if (fixPerformed)
                Console.WriteLine(Properties.Resources.FixLKGPerformed);
            else
                Console.WriteLine(Properties.Resources.FixLKGNotNeeded);
        }

        static void HandleAppUpdate()
        {
            Console.WriteLine(Properties.Resources.CheckingAppUpdates);
            var lri = UpdateCheck.GetLatestReleaseInfo();
            if (lri == null || !UpdateCheck.IsAppOutdated(lri.tag_name))
                Console.WriteLine(Properties.Resources.NoNewerVersionFound);
            else
                Console.WriteLine(Properties.Resources.NewAppUpdateDisplay, lri.name, lri.published_at, lri.body, lri.html_url);
        }

        static void HandleDictUpdate()
        {
            Console.WriteLine(Properties.Resources.CheckingDictUpdates);
            var ldi = UpdateCheck.GetLatestDictionaryInfo();
            if (ldi == null || !UpdateCheck.IsDictOutdated(ldi.sha))
                Console.WriteLine(Properties.Resources.NoNewerVersionFound);
            else
            {
                Console.WriteLine(Properties.Resources.NewDictUpdateDisplay, ldi.sha);
                var dlNew = ConsoleEx.UserQuestion(Properties.Resources.DictUpdateConsent);
                if (dlNew)
                {
                    UpdateCheck.ReplaceDict(ldi.download_url);
                    Console.WriteLine(Properties.Resources.DictUpdateFinished);
                }
                else
                    Console.WriteLine(Properties.Resources.DictUpdateCanceled);
            }
        }
        #endregion

        #region Helpers
        static RTL_FEATURE_CONFIGURATION_UPDATE[] FindResettables(bool fullReset)
        {
            var useBothStores = ArgumentBlock.ShouldUseBothStores;

            var dupeCheckHs = new HashSet<string>();
            var updateList = new List<RTL_FEATURE_CONFIGURATION_UPDATE>();
            if (useBothStores || ArgumentBlock.Store.Value == FeatureConfigurationTypeEx.Runtime)
                FindResettablesInternal(RTL_FEATURE_CONFIGURATION_TYPE.Runtime, fullReset, updateList, dupeCheckHs);
            if (useBothStores || ArgumentBlock.Store.Value == FeatureConfigurationTypeEx.Boot)
                FindResettablesInternal(RTL_FEATURE_CONFIGURATION_TYPE.Boot, fullReset, updateList, dupeCheckHs);

            return updateList.ToArray();
        }

        static void FindResettablesInternal(RTL_FEATURE_CONFIGURATION_TYPE type, bool fullReset, List<RTL_FEATURE_CONFIGURATION_UPDATE> targetList, HashSet<string> alreadyFoundSet = null)
        {
            var configs = FeatureManager.QueryAllFeatureConfigurations(type);
            foreach (var cfg in configs)
            {
                if (cfg.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.ImageDefault || cfg.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.ImageOverride)
                    continue;
                if (fullReset || ArgumentBlock.IdList.Contains(cfg.FeatureId))
                {
                    if (alreadyFoundSet != null)
                    {
                        var preCount = alreadyFoundSet.Count;
                        alreadyFoundSet.Add($"{cfg.FeatureId}-{(uint)cfg.Priority}");
                        if (alreadyFoundSet.Count == preCount)
                            continue;
                    }
                    targetList.Add(new RTL_FEATURE_CONFIGURATION_UPDATE()
                    {
                        FeatureId = cfg.FeatureId,
                        Priority = cfg.Priority,
                        Operation = RTL_FEATURE_CONFIGURATION_OPERATION.ResetState
                    });
                }
            }
        }

        static void FinalizeSet(RTL_FEATURE_CONFIGURATION_UPDATE[] updates, bool isReset)
        {
            var useBothStores = ArgumentBlock.ShouldUseBothStores;
            if (useBothStores || ArgumentBlock.Store == FeatureConfigurationTypeEx.Runtime)
            {
                var result = FeatureManager.SetFeatureConfigurations(updates, RTL_FEATURE_CONFIGURATION_TYPE.Runtime);
                if (result != 0)
                {
                    ConsoleEx.WriteErrorLine(isReset ? Properties.Resources.ResetRuntimeFailed : Properties.Resources.SetRuntimeFailed,
                        GetHumanErrorDescription(result));
                    return;
                }
            }
            if (useBothStores || ArgumentBlock.Store == FeatureConfigurationTypeEx.Boot)
            {
                var result = FeatureManager.SetFeatureConfigurations(updates, RTL_FEATURE_CONFIGURATION_TYPE.Boot);
                if (result != 0)
                {
                    ConsoleEx.WriteErrorLine(isReset ? Properties.Resources.ResetBootFailed : Properties.Resources.SetBootFailed,
                        GetHumanErrorDescription(result));
                    return;
                }

                UpdateLKGStatus(BSD_FEATURE_CONFIGURATION_STATE.BootPending);
            }

            Console.WriteLine(isReset ? Properties.Resources.ResetSuccess : Properties.Resources.SetSuccess);
        }

        static void UpdateLKGStatus(BSD_FEATURE_CONFIGURATION_STATE newStatus)
        {
            var result = FeatureManager.GetBootFeatureConfigurationState(out BSD_FEATURE_CONFIGURATION_STATE state);
            if (result != 0)
                return;

            if (state != newStatus)
            {
                result = FeatureManager.SetBootFeatureConfigurationState(newStatus);
                if (result != 0)
                    ConsoleEx.WriteWarnLine(Properties.Resources.LKGUpdateFailed,
                        GetHumanErrorDescription(result),
                        state);
            }
        }

        static void SerializeConfigsToStream(BinaryWriter bw, RTL_FEATURE_CONFIGURATION[] configurations)
        {
            if (configurations != null)
            {
                bw.Write(configurations.Length);
                foreach (var feature in configurations)
                {
                    bw.Write(feature.FeatureId);
                    bw.Write(feature.CompactState);
                    bw.Write(feature.VariantPayload);
                }
            }
            else
                bw.Write(0);
        }

        static List<RTL_FEATURE_CONFIGURATION> DeserializeConfigsFromStream(BinaryReader br)
        {
            var count = br.ReadInt32();
            var configurations = new List<RTL_FEATURE_CONFIGURATION>();
            for (int i = 0; i < count; i++)
            {
                var config = new RTL_FEATURE_CONFIGURATION
                {
                    FeatureId = br.ReadUInt32(),
                    CompactState = br.ReadUInt32(),
                    VariantPayload = br.ReadUInt32()
                };
                // Ignore these as they can't be written to the Runtime store and should be purely CBS managed
                if (config.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.ImageDefault || config.Priority == RTL_FEATURE_CONFIGURATION_PRIORITY.ImageOverride)
                    continue;
                configurations.Add(config);
            }
            return configurations;
        }

        static RTL_FEATURE_CONFIGURATION_UPDATE[] ConvertConfigsToUpdates(List<RTL_FEATURE_CONFIGURATION> configurations)
        {
            var updates = new RTL_FEATURE_CONFIGURATION_UPDATE[configurations.Count];
            for (int i = 0; i < updates.Length; i++)
            {
                var config = configurations[i];
                var update = new RTL_FEATURE_CONFIGURATION_UPDATE()
                {
                    FeatureId = config.FeatureId,
                    Priority = config.Priority,
                    EnabledState = config.EnabledState,
                    EnabledStateOptions = config.IsWexpConfiguration ? RTL_FEATURE_ENABLED_STATE_OPTIONS.WexpConfig : RTL_FEATURE_ENABLED_STATE_OPTIONS.None,
                    Variant = config.Variant,
                    VariantPayloadKind = config.VariantPayloadKind,
                    VariantPayload = config.VariantPayload,
                    Operation = RTL_FEATURE_CONFIGURATION_OPERATION.FeatureState | RTL_FEATURE_CONFIGURATION_OPERATION.VariantState
                };
                updates[i] = update;
            }
            return updates;
        }

        static void CommandMigrationInfoTip(string oldCommand, string newCommand)
        {
            ConsoleEx.WriteWarnLine(Properties.Resources.CommandMigrationNote, oldCommand, newCommand);
        }

        static string GetHumanErrorDescription(int ntStatus, bool noTranslate = false)
        {
            var hResult = 0;
            if (!noTranslate)
                hResult = NativeMethods.RtlNtStatusToDosError(ntStatus);
            if (noTranslate || hResult == 0x13D) //ERROR_MR_MID_NOT_FOUND
                hResult = ntStatus;
            var w32ex = new Win32Exception(hResult);
            return w32ex.Message;
        }
        #endregion

        #region Console printing
        static void PrintFeatureConfig(RTL_FEATURE_CONFIGURATION config, string name = null)
        {
            var defaultFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[{0}]", config.FeatureId);
            if (!string.IsNullOrEmpty(name))
                Console.Write(" ({0})", name);
            Console.WriteLine();
            Console.ForegroundColor = defaultFg;
            Console.WriteLine(Properties.Resources.FeatureDisplay_Priority, config.Priority, (uint)config.Priority);
            Console.WriteLine(Properties.Resources.FeatureDisplay_State, config.EnabledState, (uint)config.EnabledState);
            Console.WriteLine(Properties.Resources.FeatureDisplay_Type,
                config.IsWexpConfiguration ? Properties.Resources.FeatureType_Experiment : Properties.Resources.FeatureType_Override,
                config.IsWexpConfiguration ? 1 : 0);
            if (config.HasSubscriptions)
                Console.WriteLine(Properties.Resources.FeatureDisplay_HasSubscriptions, config.HasSubscriptions);
            if (config.Variant != 0)
                Console.WriteLine(Properties.Resources.FeatureDisplay_Variant, config.Variant);
            var vpkDefined = config.VariantPayloadKind != 0;
            if (vpkDefined)
                Console.WriteLine(Properties.Resources.FeatureDisplay_PayloadKind, config.VariantPayloadKind, (uint)config.VariantPayloadKind);
            if (vpkDefined || config.VariantPayload != 0)
                Console.WriteLine(Properties.Resources.FeatureDisplay_Payload, config.VariantPayload);
            Console.WriteLine();
        }

        static void PrintSubscription(RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS sub, string name = null)
        {
            var defaultFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[{0}]", sub.FeatureId);
            if (!string.IsNullOrEmpty(name))
                Console.Write(" ({0})", name);
            Console.WriteLine();
            Console.ForegroundColor = defaultFg;
            Console.WriteLine(Properties.Resources.SubscriptionDisplay_ReportingKind, sub.ReportingKind);
            Console.WriteLine(Properties.Resources.SubscriptionDisplay_ReportingOptions, sub.ReportingOptions);
            Console.WriteLine(Properties.Resources.SubscriptionDisplay_ReportingTarget, sub.ReportingTarget);
            Console.WriteLine();
        }
        #endregion
    }
}
