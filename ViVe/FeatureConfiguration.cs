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
                if (value > 14)
                    throw new ArgumentException("Group must not be more than 14");
                _group = value;
            }
        }
        public FeatureEnabledState EnabledState
        {
            get { return _enabledState; }
            set
            {
                if ((int)value > 2)
                    throw new ArgumentException("EnabledState must not be more than 2");
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
