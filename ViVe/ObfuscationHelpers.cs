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

namespace Albacore.ViVe
{
    public static class ObfuscationHelpers
    {
        private static uint SwapBytes(uint x)
        {
            x = (x >> 16) | (x << 16);
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        private static uint RotateRight32(uint value, int shift)
        {
            return (value >> shift) | (value << (32 - shift));
        }

        public static uint ObfuscateFeatureId(uint featureId)
        {
            return RotateRight32(SwapBytes(featureId ^ 0x74161A4E) ^ 0x8FB23D4F, -1) ^ 0x833EA8FF;
        }

        public static uint DeobfuscateFeatureId(uint featureId)
        {
            return SwapBytes(RotateRight32(featureId ^ 0x833EA8FF, 1) ^ 0x8FB23D4F) ^ 0x74161A4E;
        }
    }
}
