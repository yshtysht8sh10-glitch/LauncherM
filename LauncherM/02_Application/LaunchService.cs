using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using LauncherM.Domain;

namespace LauncherM.Application
{
    public sealed class LaunchService
    {
        private readonly Func<ProcessStartInfo, Process> start;
        public LaunchService() : this(info => Process.Start(info)) { }
        public LaunchService(Func<ProcessStartInfo, Process> start) { this.start = start ?? throw new ArgumentNullException(nameof(start)); }
        public LaunchService(Action<ProcessStartInfo> start) : this(info => { start(info); return null; }) { if (start == null) throw new ArgumentNullException(nameof(start)); }

        public Process Launch(LaunchItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Target)) throw new InvalidOperationException("Targetが未設定です。");
            ProcessStartInfo psi;
            if (item.Type == LaunchItemType.Folder) psi = new ProcessStartInfo("explorer.exe", Quote(item.Target));
            else if (item.Type == LaunchItemType.Url) psi = CreateUrlStartInfo(item);
            else if (item.Type == LaunchItemType.File) psi = new ProcessStartInfo(item.Target) { UseShellExecute = true };
            else if (item.Type == LaunchItemType.Command) psi = new ProcessStartInfo("cmd.exe", "/k " + item.Target + (string.IsNullOrWhiteSpace(item.Arguments) ? "" : " " + item.Arguments));
            else psi = new ProcessStartInfo(item.Target, item.Arguments ?? "") { WorkingDirectory = item.WorkingDirectory ?? "" };
            return start(psi);
        }
        private static ProcessStartInfo CreateUrlStartInfo(LaunchItem item)
        {
            string browser = string.IsNullOrWhiteSpace(item.Browser) ? "Default" : item.Browser;
            if (browser.Equals("Default", StringComparison.OrdinalIgnoreCase)) return new ProcessStartInfo(item.Target) { UseShellExecute = true };
            string executable = FindBrowser(browser); if (executable == null) throw new FileNotFoundException(browser + "が見つかりません。", browser);
            string args = ""; if (browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase) || browser.Equals("Edge", StringComparison.OrdinalIgnoreCase)) { if (!string.IsNullOrWhiteSpace(item.BrowserProfile)) args += " --profile-directory=" + Quote(item.BrowserProfile); args += " " + Quote(item.Target); } else if (browser.Equals("Firefox", StringComparison.OrdinalIgnoreCase)) { if (!string.IsNullOrWhiteSpace(item.BrowserProfile)) args += " -P " + Quote(item.BrowserProfile); args += " " + Quote(item.Target); } else throw new InvalidOperationException("未対応のブラウザです: " + browser);
            return new ProcessStartInfo(executable, args);
        }
        private static string FindBrowser(string browser)
        {
            string name, relative;
            if (browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase)) { name = "chrome.exe"; relative = @"Google\Chrome\Application"; }
            else if (browser.Equals("Edge", StringComparison.OrdinalIgnoreCase)) { name = "msedge.exe"; relative = @"Microsoft\Edge\Application"; }
            else if (browser.Equals("Firefox", StringComparison.OrdinalIgnoreCase)) { name = "firefox.exe"; relative = "Mozilla Firefox"; }
            else return null;

            // A 32-bit launcher must also inspect 64-bit browser registrations.
            foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                    try
                    {
                        using (var root = RegistryKey.OpenBaseKey(hive, view))
                        using (var key = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + name))
                        {
                            string path = (key?.GetValue(null) as string)?.Trim().Trim('"');
                            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) return path;
                        }
                    }
                    catch (Exception ex) when (ex is System.Security.SecurityException || ex is UnauthorizedAccessException || ex is IOException) { }

            string[] roots = {
                Environment.GetEnvironmentVariable("ProgramW6432"),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };
            foreach (string root in roots)
            {
                if (string.IsNullOrWhiteSpace(root)) continue;
                string candidate = Path.Combine(root, relative, name);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }
        public List<string> LaunchWorkspace(Workspace workspace)
        {
            return LaunchWorkspace(workspace, null);
        }
        public List<string> LaunchWorkspace(Workspace workspace, Action<LaunchItem, Process> launched)
        {
            var failures = new List<string>();
            var handled = new HashSet<LaunchItem>();
            foreach (var item in workspace.LaunchItems)
            {
                if (!handled.Add(item)) continue;
                if (!IsGrouped(item))
                {
                    try { Process process = Launch(item); if (launched != null) launched(item, process); } catch (Exception ex) { failures.Add(item.Name + ": " + ex.Message); }
                    continue;
                }
                var group = workspace.LaunchItems.Where(other => IsGrouped(other)
                    && string.Equals(item.Browser, other.Browser, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(item.BrowserProfile ?? "", other.BrowserProfile ?? "", StringComparison.Ordinal)
                    && string.Equals(item.BrowserWindowGroup.Trim(), other.BrowserWindowGroup.Trim(), StringComparison.Ordinal)).ToList();
                var valid = new List<LaunchItem>();
                foreach (var member in group)
                {
                    handled.Add(member);
                    Uri url;
                    if (string.IsNullOrWhiteSpace(member.Target) || !Uri.TryCreate(member.Target, UriKind.Absolute, out url))
                        failures.Add(member.Name + ": 有効なURLが未設定です。");
                    else valid.Add(member);
                }
                if (valid.Count == 0) continue;
                try
                {
                    var info = CreateUrlStartInfo(valid[0]);
                    info.Arguments = "--new-window "
                        + (string.IsNullOrWhiteSpace(item.BrowserProfile) ? "" : "--profile-directory=" + Quote(item.BrowserProfile) + " ")
                        + string.Join(" ", valid.Select(member => Quote(member.Target)));
                    Process process = start(info);
                    if (launched != null) foreach (var member in valid) launched(member, process);
                }
                catch (Exception ex) { foreach (var member in valid) failures.Add(member.Name + ": " + ex.Message); }
            }
            return failures;
        }
        private static bool IsGrouped(LaunchItem item)
        {
            return item.Type == LaunchItemType.Url && !string.IsNullOrWhiteSpace(item.BrowserWindowGroup)
                && (string.Equals(item.Browser, "Chrome", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(item.Browser, "Edge", StringComparison.OrdinalIgnoreCase));
        }
        // Windows argv quoting: double trailing backslashes and those preceding a quote.
        private static string Quote(string value)
        {
            var result = new StringBuilder("\"");
            int slashes = 0;
            foreach (char c in value)
            {
                if (c == '\\') { slashes++; continue; }
                result.Append('\\', c == '\"' ? slashes * 2 + 1 : slashes);
                result.Append(c);
                slashes = 0;
            }
            result.Append('\\', slashes * 2);
            return result.Append('\"').ToString();
        }
    }
}
