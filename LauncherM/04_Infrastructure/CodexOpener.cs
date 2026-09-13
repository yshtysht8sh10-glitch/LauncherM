using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace LauncherM.Infrastructure
{
    public static class CodexOpener
    {
        public const string OpenerName = "Codex";
        public const string NewThreadDestination = "New Thread";
        public const string UriScheme = "codex";

        public static ProcessStartInfo CreateStartInfo(string folderPath, string destination = NewThreadDestination)
        {
            return CreateStartInfo(folderPath, destination, IsUriSchemeRegistered);
        }

        public static ProcessStartInfo CreateStartInfo(string folderPath, string destination, Func<string, bool> schemeRegistered)
        {
            string path = Environment.ExpandEnvironmentVariables((folderPath ?? "").Trim().Trim('"'));
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || !Directory.Exists(path))
                throw new DirectoryNotFoundException("Codexで開くFolderが見つかりません: " + path);
            if (!string.IsNullOrWhiteSpace(destination) && !destination.Equals(NewThreadDestination, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("未対応のCodex Destinationです: " + destination);
            if (schemeRegistered == null || !schemeRegistered(UriScheme))
                throw new InvalidOperationException("Codexがインストールされていないか、codex URI Schemeが登録されていません。");

            string absolutePath = Path.GetFullPath(path);
            string uri = UriScheme + "://threads/new?path=" + Uri.EscapeDataString(absolutePath);
            return new ProcessStartInfo(uri) { UseShellExecute = true };
        }

        public static bool IsUriSchemeRegistered(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme)) return false;
            foreach (RegistryHive hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                    try
                    {
                        using (RegistryKey root = RegistryKey.OpenBaseKey(hive, view))
                        using (RegistryKey key = root.OpenSubKey(@"Software\Classes\" + scheme))
                            if (key != null && key.GetValue("URL Protocol") != null) return true;
                    }
                    catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is System.Security.SecurityException) { }
            return false;
        }
    }
}
