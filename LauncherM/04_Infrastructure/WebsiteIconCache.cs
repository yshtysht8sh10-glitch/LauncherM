using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace LauncherM.Infrastructure
{
    public sealed class WebsiteIconCache
    {
        private readonly string cacheDirectory;

        public WebsiteIconCache()
        {
            cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LauncherM", "WebsiteIcons");
        }

        public string GetOrDownload(string target)
        {
            Uri page;
            if (!Uri.TryCreate(target, UriKind.Absolute, out page)
                || (page.Scheme != Uri.UriSchemeHttp && page.Scheme != Uri.UriSchemeHttps)) return null;

            string origin = page.GetLeftPart(UriPartial.Authority);
            string path = Path.Combine(cacheDirectory, Hash(origin) + ".icon");
            if (IsUsableImage(path)) return path;

            Directory.CreateDirectory(cacheDirectory);
            Uri[] candidates = {
                new Uri(page, "/favicon.ico"),
                new Uri("https://www.google.com/s2/favicons?domain_url=" + Uri.EscapeDataString(origin) + "&sz=64")
            };
            foreach (Uri favicon in candidates)
            {
                try
                {
                    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) })
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 LauncherM/0.1");
                        byte[] bytes = client.GetByteArrayAsync(favicon).GetAwaiter().GetResult();
                        if (bytes.Length == 0 || bytes.Length > 2 * 1024 * 1024) continue;
                        File.WriteAllBytes(path, bytes);
                    }
                    if (IsUsableImage(path)) return path;
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is UnauthorizedAccessException || ex is System.Threading.Tasks.TaskCanceledException) { }
            }

            try { if (File.Exists(path)) File.Delete(path); } catch (IOException) { }
            return null;
        }

        public string GetCachedPath(string target)
        {
            Uri page;
            if (!Uri.TryCreate(target, UriKind.Absolute, out page)
                || (page.Scheme != Uri.UriSchemeHttp && page.Scheme != Uri.UriSchemeHttps)) return null;
            string path = Path.Combine(cacheDirectory, Hash(page.GetLeftPart(UriPartial.Authority)) + ".icon");
            return IsUsableImage(path) ? path : null;
        }

        public bool IsManagedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            string full = Path.GetFullPath(path);
            string root = Path.GetFullPath(cacheDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUsableImage(string path)
        {
            if (!File.Exists(path)) return false;
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return true;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is NotSupportedException || ex is System.Security.SecurityException) { return false; }
        }

        private static string Hash(string value)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value.ToLowerInvariant()));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
