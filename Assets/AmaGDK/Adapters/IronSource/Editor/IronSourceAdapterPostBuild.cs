using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor;

namespace Amanotes.Core
{
    internal class IronSourceAdapterPostBuild : EditorWindow
    {
        private const string SK_AD_NETWORK_IDS_URL =
            "https://developers.is.com/ironsource-mobile/ios/managing-skadnetwork-ids/";

        private const string AMA_GDK_EDITOR = "Assets/AmaGDK/Editor/";
        private const string SK_AD_NETWORK_LOCAL_PATH = AMA_GDK_EDITOR + "SKAdNetwork.txt";

        [PostProcessBuild(9999)]
        public static void IOSPostProcessing_WriteAdkNetwork(BuildTarget buildTarget, string pathToBuiltProject)
        {
            Task.Run(() => GetPlistData(pathToBuiltProject));
        }

        private static async Task GetPlistData(string pathToBuiltProject)
        {
            List<string> skAdNetworkList = await FetchSkAdNetworkData();
            if (skAdNetworkList != null)
            {
                WritePlist(pathToBuiltProject, skAdNetworkList);
            }
            else
            {
                WritePlistFromLocalData(pathToBuiltProject);
            }
        }

        private static async Task<List<string>> FetchSkAdNetworkData()
        {
            string rawData = await HttpGetAsync(SK_AD_NETWORK_IDS_URL);
            List<string> networkList = ExtractSkAdNetworks(rawData);

            if (networkList == null || networkList.Count == 0)
            {
                Debug.LogWarning($"AmaGDK | Fetch SKAdNetwork Data: Failed to fetch data from {SK_AD_NETWORK_IDS_URL}");
                return null;
            }

            networkList = networkList.Distinct().ToList();

            if (!Directory.Exists(AMA_GDK_EDITOR))
                Directory.CreateDirectory(AMA_GDK_EDITOR);

            File.WriteAllLines(SK_AD_NETWORK_LOCAL_PATH, networkList);
            Debug.Log(
                $"AmaGDK | Fetch SKAdNetwork Data: Successfully fetched {networkList.Count} SKAdNetworks and saved to {SK_AD_NETWORK_LOCAL_PATH}");
            return networkList;
        }

        private static void WritePlistFromLocalData(string pathToBuiltProject)
        {
            if (!File.Exists(SK_AD_NETWORK_LOCAL_PATH))
            {
                Debug.LogError($"{SK_AD_NETWORK_LOCAL_PATH} is not found!.");
                return;
            }

            List<string> skAdNetworkList = new List<string>(File.ReadAllLines(SK_AD_NETWORK_LOCAL_PATH));

            WritePlist(pathToBuiltProject, skAdNetworkList);
        }

        private static void WritePlist(string pathToBuiltProject, List<string> skAdNetworkList)
        {
            if (skAdNetworkList == null || skAdNetworkList.Count == 0)
            {
                Debug.LogWarning("AmaGDK | SKAdNetwork list is empty or null.");
                return;
            }

            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            if (!File.Exists(plistPath))
            {
                Debug.LogError($"AmaGDK | Info.plist not found at path: {plistPath}");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            var rootDict = plist.root;
            var adArray = rootDict.values.TryGetValue("SKAdNetworkItems", out var value)
                ? value as PlistElementArray
                : rootDict.CreateArray("SKAdNetworkItems");

            if (adArray == null)
            {
                Debug.LogError("AmaGDK | Failed to create or access SKAdNetworkItems array.");
                return;
            }

            var logSb = new StringBuilder();
            HashSet<string> existIds = ExtractExistingIds(adArray, logSb);
            AddNewIds(adArray, skAdNetworkList, existIds, logSb);

            try
            {
                File.WriteAllText(plistPath, plist.WriteToString());
                Debug.Log($"AmaGDK | Successfully added {adArray.values.Count} SKAdNetworkItems to the file {plistPath}. \n {logSb}");
            }
            catch (Exception e)
            {
                Debug.Log($"AmaGDK | Error occurred while writing to the file {plistPath}, Exception: {e.Message}");
            }
        }

        private static HashSet<string> ExtractExistingIds(PlistElementArray adArray, StringBuilder logger = null)
        {
            logger?.AppendLine("----- Existed -----");
            var existingIds = new HashSet<string>();

            foreach (var element in adArray.values)
            {
                if (!(element is PlistElementDict dict)) continue;
                if(!dict.values.TryGetValue("SKAdNetworkIdentifier", out var idElement))
                    continue;
                if (!(idElement is PlistElementString idString)) continue;
                existingIds.Add(idString.value);
                logger?.AppendLine(idString.value);
            }

            return existingIds;
        }

        private static void AddNewIds(PlistElementArray adArray, IEnumerable<string> skAdNetworkList, HashSet<string> existingIds, StringBuilder logger = null)
        {
            logger?.AppendLine("\n ----- New -----");
            foreach (var skAdNetwork in skAdNetworkList.Where(item => !string.IsNullOrWhiteSpace(item)))
            {
                var network = skAdNetwork.Trim();
                if (existingIds.Contains(network))
                    continue;
                var dict = adArray.AddDict();
                dict.SetString("SKAdNetworkIdentifier", network);
                existingIds.Add(network);
                logger?.AppendLine(network);
            }
        }

        private static async Task<string> HttpGetAsync(string url, int timeoutInSeconds = 15)
        {
            using HttpClient client = new HttpClient();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
            var responseTask = client.GetAsync(url, cts.Token);

            try
            {
                HttpResponseMessage response = await responseTask;
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (OperationCanceledException)
            {
                Debug.LogError($"AmaGDK | HTTP Get Timeout after {timeoutInSeconds} seconds. URL: {url}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.LogError($"AmaGDK | HTTP Get Error: {ex.Message}. URL: {url}");
                return string.Empty;
            }
            finally
            {
                cts.Dispose();
            }
        }

        private static List<string> GetAllValuesFromJson(string jsonString)
        {
            List<string> networks = new List<string>();

            string pattern = @"""([^""]*\.skadnetwork)""";
            MatchCollection matches = Regex.Matches(jsonString, pattern);

            foreach (Match match in matches)
            {
                string value = match.Groups[1].Value;
                networks.Add(value);
            }

            return networks;
        }

        private static List<string> ExtractSkAdNetworks(string data)
        {
            int startIndex = data.LastIndexOf("PLIST_NETWORK_IDS", StringComparison.Ordinal);
            if (startIndex <= 0)
            {
                return null;
            }

            int openBracketIndex = data.IndexOf("{", startIndex, StringComparison.Ordinal);
            if (openBracketIndex < 0)
            {
                return null;
            }

            int endBracketIndex = data.IndexOf("}", openBracketIndex, StringComparison.Ordinal);
            if (endBracketIndex < 0)
            {
                return null;
            }

            int length = endBracketIndex + 1 - openBracketIndex;
            var jsonData = data.Substring(openBracketIndex, length);

            return GetAllValuesFromJson(jsonData);
        }
    }
}
#endif