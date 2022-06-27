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

using System;

namespace Albacore.ViVe.NativeEnums
{
    public enum RTL_FEATURE_CONFIGURATION_TYPE : uint
    {
        Boot = 0,
        Runtime = 1
    }

    public enum RTL_FEATURE_ENABLED_STATE : uint
    {
        Default = 0,
        Disabled = 1,
        Enabled = 2
    }

    public enum RTL_FEATURE_CONFIGURATION_PRIORITY : uint
    {
        ImageDefault = 0,
        Enrollment = 2,
        Service = 4,
        User = 8,
        UserPolicy = 10,
        Test = 12,
        ImageOverride = 15
    }

    [Flags]
    public enum RTL_FEATURE_VARIANT_PAYLOAD_KIND : uint
    {
        None = 0,
        Resident = 1,
        External = 2
    }

    [Flags]
    public enum RTL_FEATURE_CONFIGURATION_OPERATION : uint
    {
        None = 0,
        FeatureState = 1,
        VariantState = 2,
        ResetState = 4
    }

    public enum RTL_FEATURE_ENABLED_STATE_OPTIONS
    {
        None = 0,
        WexpConfig = 1
    }

    public enum BSD_FEATURE_CONFIGURATION_STATE
    {
        Uninitialized = 0,
        BootPending = 1,
        LKGPending = 2,
        RollbackPending = 3,
        Committed = 4
    }
}
