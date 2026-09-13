using System;
using System.IO;
using System.Windows.Media.Imaging;
using LauncherM.Domain;
using LauncherM.Infrastructure;

class IconResolverChecks
{
    [STAThread]
    static int Main()
    {
        try
        {
            var websiteIcons = new WebsiteIconCache();
            var resolver = new IconResolver(websiteIcons);
            int checks = 0;

            string scheme;
            Check(IconResolver.TryGetUriScheme("onenote:https://d.docs.live.net/example", out scheme) && scheme == "onenote", "OneNote URI scheme", ref checks);
            Check(!IconResolver.TryGetUriScheme("https://chatgpt.com", out scheme), "Web URL is not a custom scheme", ref checks);
            string parsedCommand = IconResolver.GetCommandExecutable("\"C:\\Program Files\\Microsoft Office\\Root\\Office16\\ONENOTE.EXE\" /hyperlink \"%1\"");
            Check(string.Equals(parsedCommand, @"C:\Program Files\Microsoft Office\Root\Office16\ONENOTE.EXE", StringComparison.OrdinalIgnoreCase), "Quoted command parsing (actual: " + parsedCommand + ")", ref checks);
            string iconPath; int iconIndex;
            Check(IconResolver.TryParseIconLocation("\"%SystemRoot%\\System32\\shell32.dll\",3", out iconPath, out iconIndex) && iconIndex == 3 && iconPath.IndexOf("%SystemRoot%", StringComparison.Ordinal) < 0, "DefaultIcon parsing", ref checks);

            string exe = typeof(IconResolverChecks).Assembly.Location;
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = exe }) != null, "Executable icon", ref checks);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.File, Target = Path.GetTempFileName() }) != null, "File Shell icon", ref checks);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath() }) != null, "Folder Shell icon", ref checks);
            string code = ApplicationLocator.FindVisualStudioCode();
            Check(code == null || string.Equals(Path.GetFileName(code), "Code.exe", StringComparison.OrdinalIgnoreCase), "VS Code locator returns executable, not PATH launcher (actual: " + code + ")", ref checks);
            if (code != null)
            {
                BitmapSource folderIcon = resolver.Resolve(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Explorer" });
                BitmapSource codeIcon = resolver.Resolve(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Visual Studio Code" });
                Check(codeIcon != null && !SamePixels(folderIcon, codeIcon), "Folder VS Code uses Code.exe icon instead of Folder icon", ref checks);
            }
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Folder, Target = "missing-folder", Opener = "Application", OpenerPath = exe }) != null, "Folder explicit opener icon priority", ref checks);
            var codexFolder = new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Codex", OpenerWindowMode = "New Thread" };
            string openerScheme;
            Check(IconResolver.TryGetOpenerUriScheme(codexFolder, out openerScheme) && openerScheme == "codex", "Folder Codex opener resolves scheme icon source", ref checks);
            Check(resolver.Resolve(codexFolder) != null, "Folder Codex icon lookup safely falls back to target when association icon is unavailable", ref checks);
            BitmapSource defaultFolderIcon = resolver.Resolve(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Default" });
            BitmapSource codexIcon = resolver.Resolve(codexFolder);
            if (CodexOpener.IsUriSchemeRegistered(CodexOpener.UriScheme)) Check(codexIcon != null && !SamePixels(defaultFolderIcon, codexIcon), "Installed packaged Codex uses app icon instead of Folder icon", ref checks);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = "unknown-launcherm-scheme:value" }) == null, "Unknown scheme fallback", ref checks);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = "invalid target that does not exist.exe" }) != null, "Invalid target uses TargetType fallback", ref checks);

            string explicitImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "explicit.png");
            using (var bitmap = new System.Drawing.Bitmap(2, 2)) bitmap.Save(explicitImage, System.Drawing.Imaging.ImageFormat.Png);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = "unknown-launcherm-scheme:value", IconPath = explicitImage }) != null, "Explicit icon priority", ref checks);
            var manuallyConfigured = new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Codex", IconPath = explicitImage };
            Check(SamePixels(resolver.Resolve(manuallyConfigured), resolver.Resolve(new LaunchItem { IconPath = explicitImage })), "Manual icon remains higher priority than Codex opener", ref checks);
            manuallyConfigured.IconPathIsAutomatic = true;
            Check(!SamePixels(resolver.Resolve(manuallyConfigured), resolver.Resolve(new LaunchItem { IconPath = explicitImage })), "Automatic icon path does not hide changed opener", ref checks);
            Check(resolver.ShouldResetAutomaticIcon(manuallyConfigured, LaunchItemType.Folder, manuallyConfigured.Target, "Default", null), "Editing automatic icon from Codex to Default resets cached path", ref checks);
            manuallyConfigured.IconPathIsAutomatic = false;
            Check(!resolver.ShouldResetAutomaticIcon(manuallyConfigured, LaunchItemType.Folder, manuallyConfigured.Target, "Default", null), "Editing manual icon preserves configured path", ref checks);

            var oneNote = new LaunchItem { Type = LaunchItemType.Application, Target = "onenote:https://d.docs.live.net/example" };
            bool oneNoteRegistered = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("onenote") != null;
            Check(!oneNoteRegistered || resolver.Resolve(oneNote) != null, "Registered OneNote protocol icon", ref checks);

            string favicon = websiteIcons.GetOrDownload("https://chatgpt.com/");
            var web = new LaunchItem { Type = LaunchItemType.Url, Target = "https://chatgpt.com/", IconPath = favicon ?? "" };
            Check(favicon == null || resolver.Resolve(web) != null, "Web favicon integration", ref checks);

            string roundtripFolder = Path.Combine(Path.GetTempPath(), "LauncherM-IconResolver-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(roundtripFolder);
            try
            {
                var document = new WorkspaceDocument();
                var workspace = new Workspace { Name = "icons" };
                workspace.LaunchItems.Add(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Visual Studio Code" });
                workspace.LaunchItems.Add(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Explorer" });
                workspace.LaunchItems.Add(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Codex", IconPath = explicitImage, IconPathIsAutomatic = true });
                workspace.LaunchItems.Add(new LaunchItem { Type = LaunchItemType.Folder, Target = Path.GetTempPath(), Opener = "Visual Studio Code", IconPath = explicitImage });
                document.Workspaces.Add(workspace);
                var repository = new WorkspaceRepository(Path.Combine(roundtripFolder, "workspaces.json"));
                repository.Save(document);
                Workspace restored = repository.LoadOrMigrate(Path.Combine(roundtripFolder, "missing-de.txt")).Workspaces[0];
                var restartedResolver = new IconResolver(new WebsiteIconCache());
                Check(code == null || !SamePixels(restartedResolver.Resolve(restored.LaunchItems[0]), restartedResolver.Resolve(restored.LaunchItems[1])), "VS Code icon survives save and restart-style reload", ref checks);
                Check(restartedResolver.Resolve(restored.LaunchItems[1]) != null, "Explorer Folder icon survives save and restart-style reload", ref checks);
                Check(restartedResolver.Resolve(restored.LaunchItems[2]) != null, "Codex icon or safe Folder fallback survives save and restart-style reload", ref checks);
                Check(restored.LaunchItems[2].IconPathIsAutomatic, "Automatic icon marker survives save and restart-style reload", ref checks);
                Check(SamePixels(restartedResolver.Resolve(restored.LaunchItems[3]), restartedResolver.Resolve(new LaunchItem { IconPath = explicitImage })), "Explicit icon remains highest priority after reload", ref checks);
            }
            finally { Directory.Delete(roundtripFolder, true); }

            Console.WriteLine(checks + " checks passed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void Check(bool condition, string name, ref int checks)
    {
        if (!condition) throw new Exception(name + " failed.");
        checks++;
        Console.WriteLine("PASS " + name);
    }

    private static bool SamePixels(BitmapSource left, BitmapSource right)
    {
        if (left == null || right == null || left.PixelWidth != right.PixelWidth || left.PixelHeight != right.PixelHeight || left.Format != right.Format) return false;
        int stride = (left.PixelWidth * left.Format.BitsPerPixel + 7) / 8;
        byte[] leftPixels = new byte[stride * left.PixelHeight];
        byte[] rightPixels = new byte[stride * right.PixelHeight];
        left.CopyPixels(leftPixels, stride, 0);
        right.CopyPixels(rightPixels, stride, 0);
        if (leftPixels.Length != rightPixels.Length) return false;
        for (int i = 0; i < leftPixels.Length; i++) if (leftPixels[i] != rightPixels[i]) return false;
        return true;
    }
}
