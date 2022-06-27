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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Albacore.ViVeTool
{
    internal class FeatureNaming
    {
        internal const string DictFileName = "FeatureDictionary.pfs";
        internal static List<uint> FindIdsForNames(IEnumerable<string> featureNames)
        {
            if (!File.Exists(DictFileName))
                return null;
            var result = new List<uint>();
            var namesCommas = featureNames.Select(x => x.ToLowerInvariant() + ",").ToList();
            using (StreamReader reader = new StreamReader(File.OpenRead(DictFileName)))
            {
                while (!reader.EndOfStream)
                {
                    var currentLine = reader.ReadLine().ToLowerInvariant();
                    foreach (var nc in namesCommas)
                    {
                        if (currentLine.StartsWith(nc))
                        {
                            result.Add(uint.Parse(currentLine.Substring(nc.Length)));
                            namesCommas.Remove(nc);
                            break;
                        }
                    }
                    if (namesCommas.Count == 0)
                        break;
                }
            }
            return result;
        }

        internal static Dictionary<uint, string> FindNamesForFeatures(IEnumerable<uint> featureIDs)
        {
            var result = new Dictionary<uint, string>();
            if (!File.Exists(DictFileName))
                return null;
            var idsCommas = featureIDs.Select(x => "," + x.ToString()).ToList();
            using (StreamReader reader = new StreamReader(File.OpenRead(DictFileName)))
            {
                while (!reader.EndOfStream)
                {
                    var currentLine = reader.ReadLine();
                    foreach (var ic in idsCommas)
                    {
                        if (currentLine.EndsWith(ic))
                        {
                            result[uint.Parse(ic.Substring(1))] = currentLine.Substring(0, currentLine.IndexOf(','));
                            idsCommas.Remove(ic);
                            break;
                        }
                    }
                    if (idsCommas.Count == 0)
                        break;
                }
            }
            return result;
        }
    }
}
