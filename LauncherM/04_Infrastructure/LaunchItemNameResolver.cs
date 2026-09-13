using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LauncherM.Domain;

namespace LauncherM.Infrastructure
{
    public sealed class LaunchItemNameResolver
    {
        private static readonly Regex TitlePattern = new Regex("<title(?:\\s[^>]*)?>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly HttpClient Client = CreateClient();

        public string Resolve(LaunchItem item)
        {
            if (item == null) return "LaunchItem";
            if (!string.IsNullOrWhiteSpace(item.Name)) return item.Name.Trim();
            string target = NormalizeTarget(item.Target);
            string resolved = ResolveFromTarget(item.Type, target);
            if (!string.IsNullOrWhiteSpace(resolved)) return resolved;
            if (!string.IsNullOrWhiteSpace(item.Opener)) return item.Opener.Trim();
            if (item.Type == LaunchItemType.Url && !string.IsNullOrWhiteSpace(item.Browser) && !string.Equals(item.Browser, "Default", StringComparison.OrdinalIgnoreCase)) return item.Browser.Trim();
            return "LaunchItem";
        }

        public async Task<string> ResolveWebTitleAsync(string target, CancellationToken cancellationToken)
        {
            if (!Uri.TryCreate((target ?? "").Trim(), UriKind.Absolute, out Uri uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) return null;
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                using (HttpResponseMessage response = await Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) return null;
                    string html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Match match = TitlePattern.Match(html);
                    if (!match.Success) return null;
                    string title = Regex.Replace(WebUtility.HtmlDecode(match.Groups[1].Value), "\\s+", " ").Trim();
                    return string.IsNullOrWhiteSpace(title) ? null : title;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is UriFormatException) { return null; }
        }

        private static string ResolveFromTarget(LaunchItemType type, string target)
        {
            try
            {
                switch (type)
                {
                    case LaunchItemType.Folder: return ResolveFolderName(target);
                    case LaunchItemType.File: return Path.GetFileName(target);
                    case LaunchItemType.Application: return ResolveApplicationName(target);
                    case LaunchItemType.Url: return ResolveUriName(target);
                    default: return null;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException || ex is System.Security.SecurityException) { return null; }
        }

        private static string ResolveFolderName(string target)
        {
            string trimmed = target.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(trimmed)) return null;
            string root = Path.GetPathRoot(target);
            if (string.Equals(trimmed, (root ?? "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)) return root;
            return Path.GetFileName(trimmed);
        }

        private static string ResolveApplicationName(string target)
        {
            if (File.Exists(target))
            {
                try
                {
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(target);
                    if (!string.IsNullOrWhiteSpace(info.FileDescription)) return info.FileDescription.Trim();
                    if (!string.IsNullOrWhiteSpace(info.ProductName)) return info.ProductName.Trim();
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
                {
                }
            }
            if (LooksLikeUri(target))
            {
                string uriName = ResolveUriName(target);
                if (!string.IsNullOrWhiteSpace(uriName)) return uriName;
            }
            return Path.GetFileNameWithoutExtension(target);
        }

        private static string ResolveUriName(string target)
        {
            if (!Uri.TryCreate(target, UriKind.Absolute, out Uri uri)) return target;
            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                string host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host.Substring(4) : uri.Host;
                if (string.Equals(host, "chatgpt.com", StringComparison.OrdinalIgnoreCase)) return "ChatGPT";
                return host;
            }
            string value = Uri.UnescapeDataString(target);
            int suffix = value.IndexOfAny(new[] { '#', '?' });
            if (suffix >= 0) value = value.Substring(0, suffix);
            value = value.TrimEnd('/', '\\');
            int separator = Math.Max(value.LastIndexOf('/'), value.LastIndexOf('\\'));
            string segment = separator >= 0 ? value.Substring(separator + 1) : "";
            return string.IsNullOrWhiteSpace(segment) ? null : Path.GetFileNameWithoutExtension(segment);
        }

        private static bool LooksLikeUri(string target) { int colon = target.IndexOf(':'); return colon > 1 && Uri.TryCreate(target, UriKind.Absolute, out _); }

        private static string NormalizeTarget(string target)
        {
            return Environment.ExpandEnvironmentVariables((target ?? "").Trim().Trim('"'));
        }

        private static HttpClient CreateClient()
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = true };
            var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(8) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 LauncherM/1.0");
            return client;
        }

    }
}
