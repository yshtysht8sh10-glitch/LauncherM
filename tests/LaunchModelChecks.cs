using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LauncherM.Application;
using LauncherM.Domain;
using LauncherM.Infrastructure;
class LaunchModelChecks
{
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("PASS " + label); }
    static int Main()
    {
        var calls = new List<ProcessStartInfo>();
        var service = new LaunchService(calls.Add);
        var workspace = new Workspace { Name = "Regression" };
        foreach (LaunchItemType type in Enum.GetValues(typeof(LaunchItemType)))
            workspace.LaunchItems.Add(new LaunchItem { Name = type.ToString(), Type = type, Target = type == LaunchItemType.Url ? "https://example.com" : @"D:\sample target", Arguments = "--sample", WorkingDirectory = @"D:\work", Browser = "Default" });
        Check(service.LaunchWorkspace(workspace).Count == 0 && calls.Count == 5, "all five launch types dispatch");
        Check(calls[0].FileName == @"D:\sample target" && calls[0].Arguments == "--sample" && calls[0].WorkingDirectory == @"D:\work", "Application opener arguments and working directory");
        Check(calls[1].FileName == "explorer.exe" && calls[1].Arguments == "\"D:\\sample target\"", "Folder explorer quoting");
        Check(calls[2].FileName == @"D:\sample target" && calls[2].UseShellExecute, "File OS association");
        Check(calls[3].FileName == "https://example.com" && calls[3].UseShellExecute, "URL OS default");
        Check(calls[4].FileName == "cmd.exe" && calls[4].Arguments == @"/k D:\sample target --sample" && calls[4].WorkingDirectory == "", "Command existing lifetime and working directory limitation");
        var folder = Path.Combine(Path.GetTempPath(), "LauncherM-model-" + Guid.NewGuid()); Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "workspaces.json"); var repo = new WorkspaceRepository(path);
        var doc = new WorkspaceDocument(); doc.Workspaces.Add(workspace); repo.Save(doc);
        var loaded = new WorkspaceRepository(path).LoadOrMigrate("missing");
        for (int i = 0; i < 5; i++) {
            var before = workspace.LaunchItems[i]; var after = loaded.Workspaces[0].LaunchItems[i];
            Check(before.Id == after.Id && before.Type == after.Type && before.Target == after.Target && before.Arguments == after.Arguments && before.WorkingDirectory == after.WorkingDirectory, "restart roundtrip " + before.Type);
        }
        var legacy = Path.Combine(folder, "de.txt");
        File.WriteAllText(legacy, "2:0:D###ColonColonColon\\sample.exe:icon:visual:Legacy:memo");
        var bytes = File.ReadAllBytes(legacy);
        var migrated = new WorkspaceRepository(Path.Combine(folder, "migrated.json")).LoadOrMigrate(legacy);
        Check(migrated.Workspaces.Count == 4 && migrated.Workspaces[2].LaunchItems[0].Target == @"D:\sample.exe" && Convert.ToBase64String(bytes) == Convert.ToBase64String(File.ReadAllBytes(legacy)), "legacy migration preserves source");
        var first = new Workspace { Name = "First" }; var second = new Workspace { Name = "Second" };
        var ordered = new WorkspaceDocument(); ordered.Workspaces.Add(second); ordered.Workspaces.Add(first); repo.Save(ordered);
        var reordered = repo.LoadOrMigrate("missing");
        Check(reordered.Workspaces[0].Id == second.Id && reordered.Workspaces[1].Id == first.Id, "workspace tab order persists");
        return 0;
    }
}
