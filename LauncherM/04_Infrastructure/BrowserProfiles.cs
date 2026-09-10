using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace LauncherM.Infrastructure
{
    public sealed class BrowserProfileChoice
    {
        public string Directory { get; set; }
        public string DisplayName { get; set; }
        public string Label => string.IsNullOrEmpty(Directory) ? "未指定" : DisplayName + " (" + Directory + ")";
    }

    public static class BrowserProfiles
    {
        [DataContract] private sealed class State { [DataMember(Name = "profile")] public Profiles Profile; }
        [DataContract] private sealed class Profiles { [DataMember(Name = "info_cache")] public Dictionary<string, Entry> Cache; }
        [DataContract] private sealed class Entry { [DataMember(Name = "name")] public string Name; }

        public static List<BrowserProfileChoice> Detect(string browser, out string message)
        {
            message = "";
            var result = new List<BrowserProfileChoice>();
            string relative = browser == "Chrome" ? @"Google\Chrome\User Data" : browser == "Edge" ? @"Microsoft\Edge\User Data" : null;
            if (relative == null) return result;
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), relative);
            try
            {
                using (var stream = new FileStream(Path.Combine(root, "Local State"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    var state = (State)new DataContractJsonSerializer(typeof(State), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true }).ReadObject(stream);
                    if (state?.Profile?.Cache != null)
                        foreach (var pair in state.Profile.Cache)
                            if (Path.GetFileName(pair.Key) == pair.Key && System.IO.Directory.Exists(Path.Combine(root, pair.Key)))
                                result.Add(new BrowserProfileChoice { Directory = pair.Key, DisplayName = string.IsNullOrWhiteSpace(pair.Value?.Name) ? pair.Key : pair.Value.Name });
                }
                result.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCulture));
                if (result.Count == 0) message = "検出されたプロファイルはありません。内部Directory名を手入力できます。";
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SerializationException || ex is System.Xml.XmlException || ex is ArgumentException)
            { message = "プロファイルを検出できません。内部Directory名を手入力できます。"; }
            return result;
        }
    }
}
