using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace LauncherM.Infrastructure
{
    public static class ApplicationLocator
    {
        public static string FindVisualStudioCode()
        {
            string path = FindOnPath("code.exe");
            if (path != null) return path;

            string registered = FindAppPath("Code.exe");
            if (registered != null) return registered;

            foreach (string candidate in VisualStudioCodeCandidates())
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate)) return candidate;

            // VS Code normally adds its extensionless bin\code launcher to PATH.
            // Resolve that launcher back to the real executable so callers also get
            // the application icon instead of the Shell's generic file icon.
            string launcher = FindOnPath("code");
            if (launcher != null)
            {
                try
                {
                    string executable = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(launcher), "..", "Code.exe"));
                    if (File.Exists(executable)) return executable;
                }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException) { }
            }
            return null;
        }

        private static string FindOnPath(string command)
        {
            foreach (string directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                string root = directory.Trim().Trim('"');
                if (root.Length == 0) continue;
                try { string candidate = Path.Combine(root, command); if (File.Exists(candidate)) return candidate; }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException) { }
            }
            return null;
        }

        private static string FindAppPath(string executable)
        {
            foreach (RegistryHive hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                    try
                    {
                        using (RegistryKey root = RegistryKey.OpenBaseKey(hive, view))
                        using (RegistryKey key = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + executable))
                        {
                            string path = (key == null ? null : key.GetValue(null) as string)?.Trim().Trim('"');
                            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) return path;
                        }
                    }
                    catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is System.Security.SecurityException) { }
            return null;
        }

        private static IEnumerable<string> VisualStudioCodeCandidates()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrWhiteSpace(local)) yield return Path.Combine(local, @"Programs\Microsoft VS Code\Code.exe");
            if (!string.IsNullOrWhiteSpace(programFiles)) yield return Path.Combine(programFiles, @"Microsoft VS Code\Code.exe");
            if (!string.IsNullOrWhiteSpace(programFilesX86)) yield return Path.Combine(programFilesX86, @"Microsoft VS Code\Code.exe");
        }
    }
}
