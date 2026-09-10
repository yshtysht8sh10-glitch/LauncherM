using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LauncherM.Domain;

namespace LauncherM.Application
{
    public sealed class WorkspaceSession
    {
        public Guid WorkspaceId { get; private set; }
        public DateTime StartedAt { get; private set; }
        public List<LaunchedResource> Resources { get; private set; }
        public WorkspaceSession(Guid workspaceId) { WorkspaceId = workspaceId; StartedAt = DateTime.Now; Resources = new List<LaunchedResource>(); }
    }
    public sealed class LaunchedResource
    {
        public Guid LaunchItemId { get; private set; }
        public Process Process { get; private set; }
        public bool IncludeProcessTree { get; private set; }
        public LaunchedResource(Guid launchItemId, Process process, bool includeProcessTree) { LaunchItemId = launchItemId; Process = process; IncludeProcessTree = includeProcessTree; }
    }
    public sealed class WorkspaceSessionManager
    {
        private readonly Dictionary<Guid, WorkspaceSession> sessions = new Dictionary<Guid, WorkspaceSession>();
        public void Track(Workspace workspace, LaunchItem item, Process process)
        {
            if (workspace == null || item == null || process == null || !IsSafelyTrackable(item)) return;
            WorkspaceSession session;
            if (!sessions.TryGetValue(workspace.Id, out session)) sessions[workspace.Id] = session = new WorkspaceSession(workspace.Id);
            session.Resources.Add(new LaunchedResource(item.Id, process, item.Type == LaunchItemType.Command));
        }
        public int ActiveCount(Guid workspaceId) { WorkspaceSession session; return sessions.TryGetValue(workspaceId, out session) ? session.Resources.FindAll(resource => IsRunning(resource.Process)).Count : 0; }
        public async Task<List<string>> CloseAsync(Guid workspaceId, int gracefulWaitMilliseconds = 2500)
        {
            WorkspaceSession session; if (!sessions.TryGetValue(workspaceId, out session)) return new List<string>();
            var failures = new List<string>();
            foreach (var resource in session.Resources.ToArray())
            {
                Process process = resource.Process;
                try
                {
                    if (!IsRunning(process)) continue;
                    if (process.CloseMainWindow()) await Task.Run(() => process.WaitForExit(gracefulWaitMilliseconds));
                    if (!IsRunning(process)) continue;
                    if (resource.IncludeProcessTree) KillTree(process.Id); else process.Kill();
                    await Task.Run(() => process.WaitForExit(gracefulWaitMilliseconds));
                    if (IsRunning(process)) failures.Add("PID " + process.Id + " を終了できませんでした。");
                }
                catch (Exception ex) { failures.Add("PID " + SafeId(process) + ": " + ex.Message); }
            }
            session.Resources.RemoveAll(resource => !IsRunning(resource.Process));
            if (session.Resources.Count == 0) sessions.Remove(workspaceId);
            return failures;
        }
        public static bool IsSafelyTrackable(LaunchItem item) { return item != null && (item.Type == LaunchItemType.Application || item.Type == LaunchItemType.Command); }
        private static bool IsRunning(Process process) { try { return process != null && !process.HasExited; } catch { return false; } }
        private static int SafeId(Process process) { try { return process.Id; } catch { return -1; } }
        private static void KillTree(int processId) { using (var killer = Process.Start(new ProcessStartInfo("taskkill.exe", "/PID " + processId + " /T /F") { CreateNoWindow = true, UseShellExecute = false })) if (killer != null) killer.WaitForExit(5000); }
    }
}
