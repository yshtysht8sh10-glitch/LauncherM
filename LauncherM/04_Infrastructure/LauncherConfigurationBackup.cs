using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using LauncherM.Domain;

namespace LauncherM.Infrastructure
{
    [DataContract]
    internal sealed class LauncherConfigurationBackup
    {
        [DataMember(Order = 1)] public int Version { get; set; }
        [DataMember(Order = 2)] public DateTime ExportedAt { get; set; }
        [DataMember(Order = 3)] public WorkspaceDocument WorkspaceDocument { get; set; }
        [DataMember(Order = 4)] public LauncherPreferences Preferences { get; set; }

        internal static LauncherConfigurationBackup Create(WorkspaceDocument document)
        {
            var settings = Properties.Settings.Default;
            return new LauncherConfigurationBackup
            {
                Version = 1,
                ExportedAt = DateTime.Now,
                WorkspaceDocument = document,
                Preferences = new LauncherPreferences
                {
                    WorkspacePaneWidth = settings.WorkspacePaneWidth,
                    DetailsPaneWidth = settings.DetailsPaneWidth,
                    UiFontFamily = settings.UiFontFamily,
                    UiFontScale = settings.UiFontScale,
                    UiTheme = settings.UiTheme,
                    CardScale = settings.CardScale,
                    LaunchHotkeyEnabled = settings.LaunchHotkeyEnabled,
                    LaunchHotkeyModifiers = settings.LaunchHotkeyModifiers,
                    LaunchHotkeyKey = settings.LaunchHotkeyKey
                }
            };
        }

        internal static void Save(string path, LauncherConfigurationBackup backup)
        {
            using (FileStream stream = File.Create(path)) new DataContractJsonSerializer(typeof(LauncherConfigurationBackup)).WriteObject(stream, backup);
        }

        internal static LauncherConfigurationBackup Load(string path)
        {
            LauncherConfigurationBackup backup;
            using (FileStream stream = File.OpenRead(path)) backup = (LauncherConfigurationBackup)new DataContractJsonSerializer(typeof(LauncherConfigurationBackup)).ReadObject(stream);
            if (backup == null || backup.Version != 1 || backup.WorkspaceDocument == null || backup.Preferences == null || backup.WorkspaceDocument.Workspaces == null || backup.WorkspaceDocument.Workspaces.Count == 0) throw new InvalidDataException("LauncherMの有効な設定バックアップではありません。");
            foreach (Workspace workspace in backup.WorkspaceDocument.Workspaces) if (workspace == null || workspace.LaunchItems == null) throw new InvalidDataException("Workspaceデータが破損しています。");
            LauncherPreferences preferences = backup.Preferences;
            if (preferences.WorkspacePaneWidth <= 0 || preferences.DetailsPaneWidth <= 0 || preferences.UiFontScale < .5 || preferences.UiFontScale > 2 || preferences.CardScale < .6 || preferences.CardScale > 1.8 || string.IsNullOrWhiteSpace(preferences.UiFontFamily) || string.IsNullOrWhiteSpace(preferences.UiTheme) || string.IsNullOrWhiteSpace(preferences.LaunchHotkeyModifiers) || string.IsNullOrWhiteSpace(preferences.LaunchHotkeyKey)) throw new InvalidDataException("設定値が破損しています。");
            return backup;
        }

        internal void ApplyPreferences()
        {
            var settings = Properties.Settings.Default;
            settings.WorkspacePaneWidth = Preferences.WorkspacePaneWidth;
            settings.DetailsPaneWidth = Preferences.DetailsPaneWidth;
            settings.UiFontFamily = Preferences.UiFontFamily;
            settings.UiFontScale = Preferences.UiFontScale;
            settings.UiTheme = Preferences.UiTheme;
            settings.CardScale = Preferences.CardScale;
            settings.LaunchHotkeyEnabled = Preferences.LaunchHotkeyEnabled;
            settings.LaunchHotkeyModifiers = Preferences.LaunchHotkeyModifiers;
            settings.LaunchHotkeyKey = Preferences.LaunchHotkeyKey;
            settings.Save();
        }
    }

    [DataContract]
    internal sealed class LauncherPreferences
    {
        [DataMember(Order = 1)] public double WorkspacePaneWidth { get; set; }
        [DataMember(Order = 2)] public double DetailsPaneWidth { get; set; }
        [DataMember(Order = 3)] public string UiFontFamily { get; set; }
        [DataMember(Order = 4)] public double UiFontScale { get; set; }
        [DataMember(Order = 5)] public string UiTheme { get; set; }
        [DataMember(Order = 6)] public double CardScale { get; set; }
        [DataMember(Order = 7)] public bool LaunchHotkeyEnabled { get; set; }
        [DataMember(Order = 8)] public string LaunchHotkeyModifiers { get; set; }
        [DataMember(Order = 9)] public string LaunchHotkeyKey { get; set; }
    }
}
