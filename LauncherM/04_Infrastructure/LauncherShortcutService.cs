using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LauncherM.Infrastructure
{
    internal static class LauncherShortcutService
    {
        internal static string ShortcutPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "LauncherM.lnk"); }
        }

        internal static bool Apply(bool enabled, string modifiers, string key, out string error)
        {
            error = null;
            try
            {
                if (!enabled)
                {
                    if (File.Exists(ShortcutPath)) File.Delete(ShortcutPath);
                    return true;
                }

                string executable = Assembly.GetExecutingAssembly().Location;
                object shellObject = null;
                object shortcutObject = null;
                try
                {
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType == null) throw new InvalidOperationException("Windows Script Hostを利用できません。");
                    shellObject = Activator.CreateInstance(shellType);
                    dynamic shell = shellObject;
                    shortcutObject = shell.CreateShortcut(ShortcutPath);
                    dynamic shortcut = shortcutObject;
                    shortcut.TargetPath = executable;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(executable);
                    shortcut.Description = "LauncherMを起動";
                    shortcut.IconLocation = executable + ",0";
                    shortcut.Hotkey = modifiers + "+" + key;
                    shortcut.Save();
                    return true;
                }
                finally
                {
                    if (shortcutObject != null && Marshal.IsComObject(shortcutObject)) Marshal.FinalReleaseComObject(shortcutObject);
                    if (shellObject != null && Marshal.IsComObject(shellObject)) Marshal.FinalReleaseComObject(shellObject);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
