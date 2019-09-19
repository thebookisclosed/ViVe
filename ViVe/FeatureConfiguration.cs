using System;

namespace Albacore.ViVe
{
    public class FeatureConfiguration
    {
        private int _group;
        private FeatureEnabledState _enabledState;
        private int _enabledStateOptions;
        private int _variant;
        private int _variantPayloadKind;
        private FeatureConfigurationAction _action;

        public uint FeatureId { get; set; }
        public int Group
        {
            get { return _group; }
            set {
                if (value > 15)
                    throw new ArgumentException("Group must not be more than 15");
                _group = value;
            }
        }
        public FeatureEnabledState EnabledState
        {
            get { return _enabledState; }
            set
            {
                if ((int)value > 3)
                    throw new ArgumentException("EnabledState must not be more than 3");
                _enabledState = value;
            }
        }
        public int EnabledStateOptions
        {
            get { return _enabledStateOptions; }
            set
            {
                if (value > 1)
                    throw new ArgumentException("EnabledStateOptions must not be more than 1");
                _enabledStateOptions = value;
            }
        }
        public int Variant
        {
            get { return _variant; }
            set
            {
                if (value > 63)
                    throw new ArgumentException("Variant must not be more than 63");
                _variant = value;
            }
        }
        public int VariantPayloadKind
        {
            get { return _variantPayloadKind; }
            set
            {
                if (value > 3)
                    throw new ArgumentException("VariantPayloadKind must not be more than 3");
                _variantPayloadKind = value;
            }
        }
        public int VariantPayload { get; set; }
        public FeatureConfigurationAction Action
        {
            get { return _action; }
            set
            {
                if ((int)value > 4)
                    throw new ArgumentException("Invalid feature configuration action");
                _action = value;
            }
        }
    }
}
