using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LauncherM.Application;
using LauncherM.Domain;
using LauncherM.Infrastructure;

class BrowserWorkspaceChecks
{
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("PASS " + label); }
    static LaunchItem Url(string name, string browser = "Chrome", string profile = "Default", string group = "Main")
    { return new LaunchItem { Name = name, Type = LaunchItemType.Url, Target = "https://example.com/#" + name, Browser = browser, BrowserProfile = profile, BrowserWindowGroup = group }; }
    static int Main(string[] args)
    {
        var calls = new List<ProcessStartInfo>();
        var service = new LaunchService(info => calls.Add(info));
        var w = new Workspace();
        w.LaunchItems.AddRange(new[] { Url("A"), Url("C", group: "Research"), Url("B"), Url("D", "Edge"), Url("E", profile: "Profile 2"), Url("Legacy", "Default", null, null) });
        Check(service.LaunchWorkspace(w).Count == 0, "batch success");
        Check(calls.Count == 5, "browser/profile/group separation");
        Check(calls[0].Arguments.Contains("#A") && calls[0].Arguments.Contains("#B") && !calls[0].Arguments.Contains("#C"), "A+B command");
        Check(calls[0].Arguments.StartsWith("--new-window ") && calls[1].Arguments.Contains("#C"), "separate window command");
        Check(calls[4].UseShellExecute && calls[4].FileName.EndsWith("#Legacy"), "legacy OS default");
        calls.Clear(); service.Launch(w.LaunchItems[0]);
        Check(calls.Count == 1 && calls[0].Arguments.Contains("--profile-directory=\"Default\"") && !calls[0].Arguments.Contains("--new-window"), "individual profile");
        var broken = Url("broken"); broken.Target = ""; w.LaunchItems.Insert(0, broken);
        calls.Clear(); Check(service.LaunchWorkspace(w).Count == 1 && calls.Count == 5, "bad member does not block siblings");
        int attempts = 0;
        var failing = new LaunchService(info => { attempts++; if (attempts == 1) throw new IOException("simulated"); });
        Check(failing.LaunchWorkspace(w).Count == 3 && attempts == 5, "group failure continues subsequent groups");
        string folder = Path.Combine(Path.GetTempPath(), "LauncherM-check-" + Guid.NewGuid()); Directory.CreateDirectory(folder);
        var repo = new WorkspaceRepository(Path.Combine(folder, "workspaces.json")); var doc = new WorkspaceDocument(); doc.Workspaces.Add(w); repo.Save(doc);
        var restored = new WorkspaceRepository(Path.Combine(folder, "workspaces.json")).LoadOrMigrate("missing");
        Check(restored.Version == 1 && restored.Workspaces[0].LaunchItems[1].BrowserWindowGroup == "Main" && restored.Workspaces[0].LaunchItems[5].BrowserProfile == "Profile 2", "JSON roundtrip stable IDs");
        File.WriteAllText(Path.Combine(folder, "workspaces.json"), "{\"Version\":1,\"Workspaces\":[{\"Name\":\"old\",\"LaunchItems\":[{\"Name\":\"old\",\"Type\":3,\"Target\":\"https://example.com\"}]}]}");
        calls.Clear(); service.LaunchWorkspace(repo.LoadOrMigrate("missing").Workspaces[0]);
        Check(calls.Count == 1 && calls[0].FileName == "https://example.com" && calls[0].UseShellExecute, "missing optional JSON members");
        foreach (var browser in new[] { "Chrome", "Edge" })
        {
            string message; var profiles = BrowserProfiles.Detect(browser, out message);
            Check(profiles.Count > 0 && profiles.All(p => !string.IsNullOrEmpty(p.Directory) && !string.IsNullOrEmpty(p.Label)), browser + " real profile metadata detection");
            if (args.Contains("--launch"))
            {
                var real = new Workspace { Name = "Browser QA " + browser };
                real.LaunchItems.AddRange(new[] { Url(browser + "-A", browser, profiles[0].Directory), Url(browser + "-B", browser, profiles[0].Directory), Url(browser + "-C", browser, profiles[0].Directory, "Research") });
                Check(new LaunchService().LaunchWorkspace(real).Count == 0, browser + " dispatch only, not visual verification");
                var realDoc = new WorkspaceDocument(); realDoc.Workspaces.Add(real);
                new WorkspaceRepository(Path.Combine(folder, "workspaces.json")).Save(realDoc);
            }
        }
        Console.WriteLine("Fixture: " + folder); return 0;
    }
}
