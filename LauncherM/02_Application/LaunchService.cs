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
            else if (item.Type == LaunchItemType.Url || item.Type == LaunchItemType.File) psi = new ProcessStartInfo(item.Target) { UseShellExecute = true };
            else if (item.Type == LaunchItemType.Command) psi = new ProcessStartInfo("cmd.exe", "/k " + item.Target + (string.IsNullOrWhiteSpace(item.Arguments) ? "" : " " + item.Arguments));
            else psi = new ProcessStartInfo(item.Target, item.Arguments ?? "") { WorkingDirectory = item.WorkingDirectory ?? "" };
            Process.Start(psi);
        }
        public List<string> LaunchWorkspace(Workspace workspace)
        {
            List<string> failures = new List<string>(); foreach (LaunchItem item in workspace.LaunchItems) try { Launch(item); } catch (Exception ex) { failures.Add(item.Name + ": " + ex.Message); } return failures;
        }
        private static string Quote(string value) { return "\"" + value.Replace("\"", "\\\"") + "\""; }
    }
}
