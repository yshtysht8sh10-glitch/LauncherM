using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;
using LauncherM.Domain;

namespace LauncherM.Infrastructure
{
    public sealed class IconResolver
    {
        private const uint ShgfiIcon = 0x000000100;
        private const uint ShgfiLargeIcon = 0x000000000;
        private const uint ShgfiUseFileAttributes = 0x000000010;
        private const uint FileAttributeDirectory = 0x00000010;
        private const uint FileAttributeNormal = 0x00000080;
        private readonly WebsiteIconCache websiteIcons;
        private readonly Dictionary<string, BitmapSource> schemeIcons = new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> missingSchemeIcons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IconResolver(WebsiteIconCache websiteIcons)
        {
            this.websiteIcons = websiteIcons ?? throw new ArgumentNullException(nameof(websiteIcons));
        }

        public BitmapSource Resolve(LaunchItem item)
        {
            if (item == null) return null;
            try
            {
                bool automaticPath = IsAutomaticIconPath(item);
                BitmapSource explicitIcon = automaticPath ? null : ResolveExplicitIcon(item.IconPath);
                if (explicitIcon != null) return explicitIcon;

                if (item.Type == LaunchItemType.Folder && !string.IsNullOrWhiteSpace(item.Opener))
                {
                    string openerScheme;
                    if (TryGetOpenerUriScheme(item, out openerScheme))
                    {
                        BitmapSource schemeIcon = ResolveUriScheme(openerScheme);
                        if (schemeIcon != null) return schemeIcon;
                    }
                    string openerPath = null;
                    if (item.Opener.Equals("Visual Studio Code", StringComparison.OrdinalIgnoreCase) || item.Opener.Equals("VS Code", StringComparison.OrdinalIgnoreCase)) openerPath = ApplicationLocator.FindVisualStudioCode();
                    else if (item.Opener.Equals("Application", StringComparison.OrdinalIgnoreCase)) openerPath = Environment.ExpandEnvironmentVariables((item.OpenerPath ?? "").Trim().Trim('"'));
                    if (!string.IsNullOrWhiteSpace(openerPath) && File.Exists(openerPath))
                    {
                        BitmapSource openerIcon = GetShellIcon(openerPath, false, false);
                        if (openerIcon != null) return openerIcon;
                    }
                }

                string target = Environment.ExpandEnvironmentVariables((item.Target ?? "").Trim().Trim('"'));
                if (File.Exists(target)) return GetShellIcon(target, false, false);
                if (Directory.Exists(target)) return GetShellIcon(target, true, false);

                Uri uri;
                if (Uri.TryCreate(target, UriKind.Absolute, out uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    {
                        string favicon = string.IsNullOrWhiteSpace(item.IconPath) ? websiteIcons.GetCachedPath(target) : item.IconPath;
                        return LoadImage(favicon);
                    }
                    if (uri.Scheme != Uri.UriSchemeFile) return ResolveUriScheme(uri.Scheme);
                }

                if (item.Type == LaunchItemType.Folder) return GetShellIcon(target, true, true);
                if (item.Type == LaunchItemType.File || item.Type == LaunchItemType.Application) return GetShellIcon(target, false, true);
            }
            catch { }
            return null;
        }

        public bool IsAutomaticIconPath(LaunchItem item)
        {
            return item != null && (item.IconPathIsAutomatic || websiteIcons.IsManagedPath(item.IconPath));
        }

        public bool ShouldResetAutomaticIcon(LaunchItem item, LaunchItemType type, string target, string opener, string openerPath)
        {
            return IsAutomaticIconPath(item) && (item.Type != type
                || !string.Equals(item.Target, target, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(item.Opener, opener, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(item.OpenerPath, openerPath, StringComparison.OrdinalIgnoreCase));
        }

        public static bool TryGetUriScheme(string target, out string scheme)
        {
            scheme = null;
            Uri uri;
            if (!Uri.TryCreate((target ?? "").Trim(), UriKind.Absolute, out uri)
                || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFile) return false;
            scheme = uri.Scheme;
            return true;
        }

        public static bool TryGetOpenerUriScheme(LaunchItem item, out string scheme)
        {
            scheme = null;
            if (item == null || item.Type != LaunchItemType.Folder
                || !string.Equals(item.Opener, CodexOpener.OpenerName, StringComparison.OrdinalIgnoreCase)) return false;
            scheme = CodexOpener.UriScheme;
            return true;
        }

        public string GetOrDownloadWebsiteIcon(LaunchItem item)
        {
            if (item == null || !string.IsNullOrWhiteSpace(item.IconPath)) return null;
            Uri uri;
            if (!Uri.TryCreate(item.Target, UriKind.Absolute, out uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) return null;
            return websiteIcons.GetOrDownload(item.Target);
        }

        private BitmapSource ResolveExplicitIcon(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            BitmapSource image = LoadImage(path);
            return image ?? GetShellIcon(path, false, false);
        }

        private BitmapSource ResolveUriScheme(string scheme)
        {
            BitmapSource cached;
            if (schemeIcons.TryGetValue(scheme, out cached)) return cached;
            if (missingSchemeIcons.Contains(scheme)) return null;

            BitmapSource icon = null;
            string progId = ReadUserChoiceProgId(scheme);
            if (!string.IsNullOrWhiteSpace(progId)) icon = ResolveRegisteredClass(progId);
            if (icon == null) icon = ResolveRegisteredClass(scheme);
            if (icon == null) icon = ResolvePackagedProtocol(scheme);
            if (icon != null) schemeIcons[scheme] = icon;
            else missingSchemeIcons.Add(scheme);
            return icon;
        }

        private static BitmapSource ResolvePackagedProtocol(string scheme)
        {
            const string packagesKey = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";
            try
            {
                using (RegistryKey packages = Registry.CurrentUser.OpenSubKey(packagesKey))
                {
                    if (packages == null) return null;
                    foreach (string packageName in packages.GetSubKeyNames())
                    using (RegistryKey package = packages.OpenSubKey(packageName))
                    using (RegistryKey associations = package?.OpenSubKey(@"App\Capabilities\URLAssociations"))
                    {
                        if (associations == null || associations.GetValue(scheme) == null) continue;
                        string root = package.GetValue("PackageRootFolder") as string;
                        BitmapSource icon = ResolvePackageManifestIcon(root, scheme);
                        if (icon != null) return icon;
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is System.Security.SecurityException) { }
            return null;
        }

        private static BitmapSource ResolvePackageManifestIcon(string packageRoot, string scheme)
        {
            try
            {
                string manifestPath = Path.Combine(packageRoot ?? "", "AppxManifest.xml");
                if (!File.Exists(manifestPath)) return null;
                XDocument manifest = XDocument.Load(manifestPath);
                foreach (XElement application in manifest.Descendants().Where(element => element.Name.LocalName == "Application"))
                {
                    bool ownsProtocol = application.Descendants().Any(element => element.Name.LocalName == "Protocol" && string.Equals((string)element.Attribute("Name"), scheme, StringComparison.OrdinalIgnoreCase));
                    if (!ownsProtocol) continue;
                    XElement visual = application.Descendants().FirstOrDefault(element => element.Name.LocalName == "VisualElements");
                    string logo = (string)visual?.Attribute("Square44x44Logo") ?? (string)visual?.Attribute("Square150x150Logo");
                    BitmapSource icon = LoadBestPackageLogo(packageRoot, logo);
                    if (icon != null) return icon;
                    string executable = (string)application.Attribute("Executable");
                    string executablePath = Path.Combine(packageRoot, (executable ?? "").Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(executablePath)) return GetShellIcon(executablePath, false, false);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Xml.XmlException || ex is ArgumentException || ex is System.Security.SecurityException) { }
            return null;
        }

        private static BitmapSource LoadBestPackageLogo(string packageRoot, string relativeLogo)
        {
            if (string.IsNullOrWhiteSpace(relativeLogo)) return null;
            string path = Path.Combine(packageRoot, relativeLogo.Replace('/', Path.DirectorySeparatorChar));
            BitmapSource image = LoadImage(path);
            if (image != null) return image;
            string directory = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            if (!Directory.Exists(directory)) return null;
            foreach (string candidate in Directory.GetFiles(directory, name + ".scale-*" + extension).OrderByDescending(File.GetLastWriteTimeUtc))
            {
                image = LoadImage(candidate);
                if (image != null) return image;
            }
            return null;
        }

        private static BitmapSource ResolveRegisteredClass(string className)
        {
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                BitmapSource icon = ResolveRegisteredClass(RegistryHive.CurrentUser, view, @"Software\Classes\" + className);
                if (icon != null) return icon;
                icon = ResolveRegisteredClass(RegistryHive.LocalMachine, view, @"Software\Classes\" + className);
                if (icon != null) return icon;
            }
            return null;
        }

        private static BitmapSource ResolveRegisteredClass(RegistryHive hive, RegistryView view, string keyPath)
        {
            try
            {
                using (RegistryKey root = RegistryKey.OpenBaseKey(hive, view))
                using (RegistryKey key = root.OpenSubKey(keyPath))
                {
                    if (key == null) return null;
                    using (RegistryKey defaultIcon = key.OpenSubKey("DefaultIcon"))
                    {
                        BitmapSource icon = ExtractRegisteredIcon(defaultIcon == null ? null : defaultIcon.GetValue(null) as string);
                        if (icon != null) return icon;
                    }
                    using (RegistryKey command = key.OpenSubKey(@"shell\open\command"))
                    {
                        string executable = GetCommandExecutable(command == null ? null : command.GetValue(null) as string);
                        if (!string.IsNullOrWhiteSpace(executable) && File.Exists(executable)) return GetShellIcon(executable, false, false);
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is System.Security.SecurityException) { }
            return null;
        }

        private static string ReadUserChoiceProgId(string scheme)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\" + scheme + @"\UserChoice"))
                    return key == null ? null : key.GetValue("ProgId") as string;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException) { return null; }
        }

        private static BitmapSource ExtractRegisteredIcon(string value)
        {
            string path;
            int index;
            if (!TryParseIconLocation(value, out path, out index) || !File.Exists(path)) return null;
            IntPtr[] large = new IntPtr[1];
            try
            {
                if (ExtractIconEx(path, index, large, null, 1) == 0 || large[0] == IntPtr.Zero) return null;
                return FromIconHandle(large[0]);
            }
            finally { if (large[0] != IntPtr.Zero) DestroyIcon(large[0]); }
        }

        public static bool TryParseIconLocation(string value, out string path, out int index)
        {
            path = null; index = 0;
            if (string.IsNullOrWhiteSpace(value) || value.TrimStart().StartsWith("@", StringComparison.Ordinal)) return false;
            string expanded = Environment.ExpandEnvironmentVariables(value.Trim());
            int comma = expanded.LastIndexOf(',');
            if (comma >= 0)
            {
                int parsed;
                if (int.TryParse(expanded.Substring(comma + 1).Trim(), out parsed)) { index = parsed; expanded = expanded.Substring(0, comma); }
            }
            path = expanded.Trim().Trim('"');
            return !string.IsNullOrWhiteSpace(path);
        }

        public static string GetCommandExecutable(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return null;
            string expanded = Environment.ExpandEnvironmentVariables(command.Trim());
            int argc;
            IntPtr argv = CommandLineToArgvW(expanded, out argc);
            if (argv == IntPtr.Zero || argc == 0) return null;
            try { return Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv))?.Trim(); }
            finally { LocalFree(argv); }
        }

        private static BitmapSource LoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            try
            {
                var image = new BitmapImage();
                image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.UriSource = new Uri(path, UriKind.Absolute); image.EndInit(); image.Freeze();
                return image;
            }
            catch (Exception ex) when (ex is IOException || ex is NotSupportedException || ex is UriFormatException) { return null; }
        }

        private static BitmapSource GetShellIcon(string path, bool directory, bool useAttributes)
        {
            ShFileInfo info;
            uint flags = ShgfiIcon | ShgfiLargeIcon | (useAttributes ? ShgfiUseFileAttributes : 0);
            IntPtr result = SHGetFileInfo(path, directory ? FileAttributeDirectory : FileAttributeNormal, out info, (uint)Marshal.SizeOf(typeof(ShFileInfo)), flags);
            if (result == IntPtr.Zero || info.Icon == IntPtr.Zero) return null;
            try { return FromIconHandle(info.Icon); }
            finally { DestroyIcon(info.Icon); }
        }

        private static BitmapSource FromIconHandle(IntPtr icon)
        {
            BitmapSource image = Imaging.CreateBitmapSourceFromHIcon(icon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            image.Freeze();
            return image;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ShFileInfo { public IntPtr Icon; public int IconIndex; public uint Attributes; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string DisplayName; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string TypeName; }
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SHGetFileInfo(string path, uint attributes, out ShFileInfo info, uint size, uint flags);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)] private static extern uint ExtractIconEx(string file, int index, IntPtr[] large, IntPtr[] small, uint count);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CommandLineToArgvW(string commandLine, out int argc);
        [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr memory);
        [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr icon);
    }
}
