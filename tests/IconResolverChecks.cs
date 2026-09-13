using System;
using System.IO;
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
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Folder, Target = "missing-folder", Opener = "Application", OpenerPath = exe }) != null, "Folder explicit opener icon priority", ref checks);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = "unknown-launcherm-scheme:value" }) == null, "Unknown scheme fallback", ref checks);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = "invalid target that does not exist" }) == null, "Invalid target fallback", ref checks);

            string explicitImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "explicit.png");
            using (var bitmap = new System.Drawing.Bitmap(2, 2)) bitmap.Save(explicitImage, System.Drawing.Imaging.ImageFormat.Png);
            Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Application, Target = "unknown-launcherm-scheme:value", IconPath = explicitImage }) != null, "Explicit icon priority", ref checks);

            var oneNote = new LaunchItem { Type = LaunchItemType.Application, Target = "onenote:https://d.docs.live.net/example" };
            bool oneNoteRegistered = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("onenote") != null;
            Check(!oneNoteRegistered || resolver.Resolve(oneNote) != null, "Registered OneNote protocol icon", ref checks);

            string favicon = websiteIcons.GetOrDownload("https://chatgpt.com/");
            var web = new LaunchItem { Type = LaunchItemType.Url, Target = "https://chatgpt.com/", IconPath = favicon ?? "" };
            Check(favicon == null || resolver.Resolve(web) != null, "Web favicon integration", ref checks);

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
}
