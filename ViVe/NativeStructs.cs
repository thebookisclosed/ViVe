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

using Albacore.ViVe.Exceptions;
using Albacore.ViVe.NativeEnums;
using System.Runtime.InteropServices;

namespace Albacore.ViVe.NativeStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_FEATURE_USAGE_REPORT
    {
        public uint FeatureId;
        public ushort ReportingKind;
        public ushort ReportingOptions;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_FEATURE_CONFIGURATION
    {
        public uint FeatureId;
        public uint CompactState;
        public uint VariantPayload;

        public RTL_FEATURE_CONFIGURATION_PRIORITY Priority
        {
            get
            {
                return (RTL_FEATURE_CONFIGURATION_PRIORITY)(CompactState & 0xF);
            }
            set
            {
                if ((uint)value > 15)
                    throw new FeaturePropertyOverflowException("Priority", 15);
                CompactState = (CompactState & 0xFFFFFFF0) | (uint)value;
            }
        }

        public RTL_FEATURE_ENABLED_STATE EnabledState
        {
            get
            {
                return (RTL_FEATURE_ENABLED_STATE)((CompactState & 0x30) >> 4);
            }
            set
            {
                if ((uint)value > 2)
                    throw new FeaturePropertyOverflowException("EnabledState", 2);
                CompactState = (CompactState & 0xFFFFFFCF) | ((uint)value << 4);
            }
        }

        public bool IsWexpConfiguration
        {
            get
            {
                return ((CompactState & 0x40) >> 6) == 1;
            }
            set
            {
                CompactState = (CompactState & 0xFFFFFFBF) | ((value ? (uint)1 : 0) << 6);
            }
        }

        public bool HasSubscriptions
        {
            get
            {
                return ((CompactState & 0x80) >> 7) == 1;
            }
            set
            {
                CompactState = (CompactState & 0xFFFFFF7F) | ((value ? (uint)1 : 0) << 7);
            }
        }

        public uint Variant
        {
            get
            {
                return (CompactState & 0x3F00) >> 8;
            }
            set
            {
                if (value > 63)
                    throw new FeaturePropertyOverflowException("Variant", 63);
                CompactState = (CompactState & 0xFFFFC0FF) | (value << 8);
            }
        }

        public RTL_FEATURE_VARIANT_PAYLOAD_KIND VariantPayloadKind
        {
            get
            {
                return (RTL_FEATURE_VARIANT_PAYLOAD_KIND)((CompactState & 0xC000) >> 14);
            }
            set
            {
                if ((uint)value > 3)
                    throw new FeaturePropertyOverflowException("VariantPayloadKind", 3);
                CompactState = (CompactState & 0xFFFF3FFF) | ((uint)value << 14);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_FEATURE_USAGE_SUBSCRIPTION_DETAILS
    {
        public uint FeatureId;
        public ushort ReportingKind;
        public ushort ReportingOptions;
        public ulong ReportingTarget;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_FEATURE_CONFIGURATION_UPDATE
    {
        public uint FeatureId;
        private RTL_FEATURE_CONFIGURATION_PRIORITY _priority;
        private RTL_FEATURE_ENABLED_STATE _enabledState;
        private RTL_FEATURE_ENABLED_STATE_OPTIONS _enabledStateOptions;
        private uint _variant;
        private RTL_FEATURE_VARIANT_PAYLOAD_KIND _variantPayloadKind;
        public uint VariantPayload;
        private RTL_FEATURE_CONFIGURATION_OPERATION _operation;

        public RTL_FEATURE_CONFIGURATION_PRIORITY Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if ((uint)value > 15)
                    throw new FeaturePropertyOverflowException("Priority", 15);
                _priority = value;
            }
        }

        public RTL_FEATURE_ENABLED_STATE EnabledState
        {
            get
            {
                return _enabledState;
            }
            set
            {
                if ((uint)value > 2)
                    throw new FeaturePropertyOverflowException("EnabledState", 2);
                _enabledState = value;
            }
        }

        public RTL_FEATURE_ENABLED_STATE_OPTIONS EnabledStateOptions
        {
            get
            {
                return _enabledStateOptions;
            }
            set
            {
                if ((uint)value > 1)
                    throw new FeaturePropertyOverflowException("EnabledStateOptions", 1);
                _enabledStateOptions = value;
            }
        }

        public uint Variant
        {
            get
            {
                return _variant;
            }
            set
            {
                if (value > 63)
                    throw new FeaturePropertyOverflowException("Variant", 63);
                _variant = value;
            }
        }

        public RTL_FEATURE_VARIANT_PAYLOAD_KIND VariantPayloadKind
        {
            get
            {
                return _variantPayloadKind;
            }
            set
            {
                if ((uint)value > 3)
                    throw new FeaturePropertyOverflowException("VariantPayloadKind", 3);
                _variantPayloadKind = value;
            }
        }

        public RTL_FEATURE_CONFIGURATION_OPERATION Operation
        {
            get
            {
                return _operation;
            }
            set
            {
                if ((uint)value > 4)
                    throw new FeaturePropertyOverflowException("Operation", 4);
                _operation = value;
            }
        }

        public bool UserPolicyPriorityCompatible
        {
            get
            {
                return !((uint)EnabledStateOptions != 0 || _variant != 0 || (uint)_variantPayloadKind != 0 || VariantPayload != 0);
            }
        }
    }
}
