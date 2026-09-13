using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LauncherM.Domain
{
    public enum LaunchItemType { Application, Folder, File, Url, Command }

    [DataContract]
    public sealed class LaunchItem
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }
        [DataMember(Order = 2)] public string Name { get; set; }
        [DataMember(Order = 3)] public LaunchItemType Type { get; set; }
        [DataMember(Order = 4)] public string Target { get; set; }
        [DataMember(Order = 5)] public string Arguments { get; set; }
        [DataMember(Order = 6)] public string WorkingDirectory { get; set; }
        [DataMember(Order = 7)] public string Browser { get; set; }
        [DataMember(Order = 8)] public string BrowserProfile { get; set; }
        [DataMember(Order = 9)] public string IconPath { get; set; }
        [DataMember(Order = 10, EmitDefaultValue = false)] public string BrowserWindowGroup { get; set; }
        [DataMember(Order = 11, EmitDefaultValue = false)] public string Opener { get; set; }
        [DataMember(Order = 12, EmitDefaultValue = false)] public string OpenerPath { get; set; }
        [DataMember(Order = 13, EmitDefaultValue = false)] public string OpenerWindowMode { get; set; }
        [DataMember(Order = 14, EmitDefaultValue = false)] public bool IconPathIsAutomatic { get; set; }
        public LaunchItem() { Id = Guid.NewGuid(); Name = ""; Target = ""; Arguments = ""; WorkingDirectory = ""; Browser = "Default"; BrowserProfile = ""; IconPath = ""; Opener = ""; OpenerPath = ""; OpenerWindowMode = "Default"; }
    }

    [DataContract]
    public sealed class Workspace
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }
        [DataMember(Order = 2)] public string Name { get; set; }
        [DataMember(Order = 3)] public List<LaunchItem> LaunchItems { get; set; }
        public Workspace() { Id = Guid.NewGuid(); Name = ""; LaunchItems = new List<LaunchItem>(); }
    }

    [DataContract]
    public sealed class WorkspaceDocument
    {
        [DataMember(Order = 1)] public int Version { get; set; }
        [DataMember(Order = 2)] public List<Workspace> Workspaces { get; set; }
        public WorkspaceDocument() { Version = 1; Workspaces = new List<Workspace>(); }
    }
}
