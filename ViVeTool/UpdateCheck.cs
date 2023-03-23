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

using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Albacore.ViVeTool
{
    internal class UpdateCheck
    {
        private const string GitHubRepoApiRoot = "https://api.github.com/repos/thebookisclosed/ViVe/";
        private static WebClient UcWebClient;

        internal static GitHubReleaseInfo GetLatestReleaseInfo()
        {
            return GetJsonResponse<GitHubReleaseInfo>("releases/latest");
        }

        internal static bool IsAppOutdated(string versionTag)
        {
            var foundVersion = new Version(versionTag.Substring(1));
            var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return foundVersion > currentVersion;
        }

        internal static GitHubRepoContent GetLatestDictionaryInfo()
        {
            var resp = GetJsonResponse<GitHubRepoContent[]>("contents/Extra");
            var dictInfo = resp?.Where(x => x.name == FeatureNaming.DictFileName).FirstOrDefault();
            return dictInfo;
        }

        internal static bool IsDictOutdated(string sha)
        {
            return sha != HashUTF8TextFile(FeatureNaming.DictFilePath);
        }

        internal static void ReplaceDict(string url)
        {
            EnsureWebClient();
            UcWebClient.DownloadFile(url, FeatureNaming.DictFilePath);
        }

        private static void EnsureWebClient()
        {
            if (UcWebClient == null)
            {
                UcWebClient = new WebClient();
                UcWebClient.Headers.Add("User-Agent", "ViVeTool");
            }
        }

        private static T GetJsonResponse<T>(string apiPath)
        {
            EnsureWebClient();
            try
            {
                var stringResponse = UcWebClient.DownloadString(GitHubRepoApiRoot + apiPath);
                return JsonConvert.DeserializeObject<T>(stringResponse);
            }
            catch { return default; }
        }

        private static string HashUTF8TextFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
            using (var sha1Csp = new SHA1CryptoServiceProvider())
            {
                var fileBody = Encoding.UTF8.GetBytes(File.ReadAllText(filePath).Replace("\r\n", "\n"));
                var fullLength = fileBody.Length;

                var preamble = Encoding.UTF8.GetPreamble();
                var filePortion = new byte[preamble.Length];
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    fs.Read(filePortion, 0, filePortion.Length);

                for (int i = 0; i < preamble.Length; i++)
                    if (preamble[i] == filePortion[i])
                        fullLength++;

                var preface = Encoding.UTF8.GetBytes($"blob {fullLength}\0");
                sha1Csp.TransformBlock(preface, 0, preface.Length, null, 0);
                if (fullLength > fileBody.Length)
                    sha1Csp.TransformBlock(preamble, 0, preamble.Length, null, 0);
                sha1Csp.TransformFinalBlock(fileBody, 0, fileBody.Length);

                return BitConverter.ToString(sha1Csp.Hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public class GitHubReleaseInfo
    {
        public string html_url { get; set; }
        public string tag_name { get; set; }
        public string name { get; set; }
        public DateTime published_at { get; set; }
        public string body { get; set; }
    }


    public class GitHubRepoContent
    {
        public string name { get; set; }
        public string sha { get; set; }
        public string download_url { get; set; }
    }
}
