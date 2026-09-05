using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using LauncherM.Domain;

namespace LauncherM.Infrastructure
{
    public sealed class WorkspaceRepository
    {
        private readonly string path;
        public WorkspaceRepository(string path) { this.path = path; }
        public WorkspaceDocument LoadOrMigrate(string legacyPath)
        {
            if (File.Exists(path)) using (FileStream s = File.OpenRead(path)) return (WorkspaceDocument)new DataContractJsonSerializer(typeof(WorkspaceDocument)).ReadObject(s);
            WorkspaceDocument doc = new WorkspaceDocument();
            for (int i = 0; i < 4; i++) doc.Workspaces.Add(new Workspace { Name = "Launcher " + (i + 1) });
            if (File.Exists(legacyPath)) foreach (string line in File.ReadAllLines(legacyPath, Encoding.GetEncoding("Shift_JIS")))
            {
                string[] e = line.Split(':'); if (e.Length < 7) continue; int group, order;
                if (!int.TryParse(e[0], out group) || !int.TryParse(e[1], out order) || group < 0 || group > 3) continue;
                doc.Workspaces[group].LaunchItems.Add(new LaunchItem { Name = Restore(e[5]), Type = LaunchItemType.Application, Target = Restore(e[2]) });
            }
            Save(doc); return doc;
        }
        public void Save(WorkspaceDocument doc) { using (FileStream s = File.Create(path)) new DataContractJsonSerializer(typeof(WorkspaceDocument)).WriteObject(s, doc); }
        private static string Restore(string s) { return (s ?? "").Replace("###ColonColonColon", ":").Replace("###CarriCarriCarri", "\r").Replace("###LinefLinefnLinef", "\n"); }
    }
}
