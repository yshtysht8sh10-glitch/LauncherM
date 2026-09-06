using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LauncherM.Domain;

namespace LauncherM.Application
{
    public sealed class LaunchService
    {
        public void Launch(LaunchItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Target)) throw new InvalidOperationException("Targetが未設定です。");
            ProcessStartInfo psi;
            if (item.Type == LaunchItemType.Folder) psi = new ProcessStartInfo("explorer.exe", Quote(item.Target));
            else if (item.Type == LaunchItemType.Url) psi = CreateUrlStartInfo(item);
            else if (item.Type == LaunchItemType.File) psi = new ProcessStartInfo(item.Target) { UseShellExecute = true };
            else if (item.Type == LaunchItemType.Command) psi = new ProcessStartInfo("cmd.exe", "/k " + item.Target + (string.IsNullOrWhiteSpace(item.Arguments) ? "" : " " + item.Arguments));
            else psi = new ProcessStartInfo(item.Target, item.Arguments ?? "") { WorkingDirectory = item.WorkingDirectory ?? "" };
            Process.Start(psi);
        }
        private static ProcessStartInfo CreateUrlStartInfo(LaunchItem item)
        {
            string browser = string.IsNullOrWhiteSpace(item.Browser) ? "Default" : item.Browser;
            if (browser.Equals("Default", StringComparison.OrdinalIgnoreCase)) return new ProcessStartInfo(item.Target) { UseShellExecute = true };
            string executable = FindBrowser(browser); if (executable == null) throw new FileNotFoundException(browser + "が見つかりません。", browser);
            string args = ""; if (browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase) || browser.Equals("Edge", StringComparison.OrdinalIgnoreCase)) { if (!string.IsNullOrWhiteSpace(item.BrowserProfile)) args += " --profile-directory=" + Quote(item.BrowserProfile); args += " " + Quote(item.Target); } else if (browser.Equals("Firefox", StringComparison.OrdinalIgnoreCase)) { if (!string.IsNullOrWhiteSpace(item.BrowserProfile)) args += " -P " + Quote(item.BrowserProfile); args += " " + Quote(item.Target); } else throw new InvalidOperationException("未対応のブラウザです: " + browser);
            return new ProcessStartInfo(executable, args);
        }
        private static string FindBrowser(string browser) { string name = browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase) ? "chrome.exe" : (browser.Equals("Edge", StringComparison.OrdinalIgnoreCase) ? "msedge.exe" : (browser.Equals("Firefox", StringComparison.OrdinalIgnoreCase) ? "firefox.exe" : null)); if (name == null) return null; string[] roots = { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) }; foreach (string root in roots) { string[] candidates = browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase) ? new[] { Path.Combine(root, "Google\\Chrome\\Application", name) } : browser.Equals("Edge", StringComparison.OrdinalIgnoreCase) ? new[] { Path.Combine(root, "Microsoft\\Edge\\Application", name) } : new[] { Path.Combine(root, "Mozilla Firefox", name) }; foreach (string candidate in candidates) if (File.Exists(candidate)) return candidate; } return null; }
        public List<string> LaunchWorkspace(Workspace workspace)
        {
            List<string> failures = new List<string>(); foreach (LaunchItem item in workspace.LaunchItems) try { Launch(item); } catch (Exception ex) { failures.Add(item.Name + ": " + ex.Message); } return failures;
        }
        private static string Quote(string value) { return "\"" + value.Replace("\"", "\\\"") + "\""; }
    }
}
