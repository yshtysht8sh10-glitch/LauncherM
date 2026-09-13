using System;
using System.Diagnostics;
using LauncherM.Application;
using LauncherM.Domain;

class WorkspaceLifecycleChecks
{
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("PASS " + label); }
    static int Main()
    {
        var workspace = new Workspace { Name = "Lifecycle" };
        var command = new LaunchItem { Name = "server", Type = LaunchItemType.Command, Target = "ping" };
        var browser = new LaunchItem { Name = "browser", Type = LaunchItemType.Url, Target = "https://example.com" };
        var codex = new LaunchItem { Name = "codex", Type = LaunchItemType.Folder, Target = @"D:\working", Opener = "Codex", OpenerWindowMode = "New Thread" };
        Check(WorkspaceSessionManager.IsSafelyTrackable(command), "commands are trackable");
        Check(!WorkspaceSessionManager.IsSafelyTrackable(browser), "browser resources are excluded");
        Check(!WorkspaceSessionManager.IsSafelyTrackable(codex), "Codex resources are excluded");

        Process tracked = Process.Start(new ProcessStartInfo("cmd.exe", "/c ping 127.0.0.1 -t") { CreateNoWindow = true, UseShellExecute = false });
        Process unrelated = Process.Start(new ProcessStartInfo("cmd.exe", "/c ping 127.0.0.1 -t") { CreateNoWindow = true, UseShellExecute = false });
        try
        {
            var manager = new WorkspaceSessionManager();
            manager.Track(workspace, command, tracked);
            manager.Track(workspace, browser, unrelated);
            Check(manager.ActiveCount(workspace.Id) == 1, "only safe launched resource is tracked");
            Check(manager.CloseAsync(workspace.Id, 250).GetAwaiter().GetResult().Count == 0, "tracked command tree closes");
            tracked.Refresh(); unrelated.Refresh();
            Check(tracked.HasExited, "tracked process exited");
            Check(!unrelated.HasExited, "unrelated process remains running");
            Check(manager.ActiveCount(workspace.Id) == 0, "workspace can start a new session");
        }
        finally
        {
            try { if (!tracked.HasExited) tracked.Kill(); } catch { }
            try { if (!unrelated.HasExited) unrelated.Kill(); } catch { }
        }
        return 0;
    }
}
