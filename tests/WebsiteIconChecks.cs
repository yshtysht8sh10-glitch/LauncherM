using System;
using System.IO;
using LauncherM.Infrastructure;

class WebsiteIconChecks
{
    static int Main()
    {
        try
        {
            var cache = new WebsiteIconCache();
            string first = cache.GetOrDownload("https://chatgpt.com/c/example");
            if (string.IsNullOrWhiteSpace(first) || !File.Exists(first)) throw new Exception("ChatGPT favicon was not cached.");
            string second = cache.GetOrDownload("https://chatgpt.com/another/page");
            if (!string.Equals(first, second, StringComparison.OrdinalIgnoreCase)) throw new Exception("Same origin did not reuse the icon.");
            if (!cache.IsManagedPath(first)) throw new Exception("Cached icon was not recognized as managed.");
            Console.WriteLine(first);
            Console.WriteLine(new FileInfo(first).Length);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.GetType().FullName); Console.Error.WriteLine(ex.Message); Console.Error.WriteLine(ex.StackTrace); return 1; }
    }
}
