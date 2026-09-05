using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LauncherM.Application;
using LauncherM.Domain;
using LauncherM.Infrastructure;

namespace LauncherM
{
    public partial class WorkspaceWindow : Window
    {
        private readonly WorkspaceDocument document;
        private readonly LaunchService launcher = new LaunchService();
        private LaunchItem selectedItem;
        public WorkspaceWindow()
        {
            InitializeComponent();
            string basePath = Directory.GetCurrentDirectory();
            document = new WorkspaceRepository(Path.Combine(basePath, "workspaces.json")).LoadOrMigrate(Path.Combine(basePath, "de.txt"));
            workspaceBox.ItemsSource = document.Workspaces;
            if (document.Workspaces.Count > 0) workspaceBox.SelectedIndex = 0;
        }
        private void WorkspaceChanged(object sender, SelectionChangedEventArgs e)
        {
            itemsPanel.Children.Clear(); Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return;
            foreach (LaunchItem item in workspace.LaunchItems) { StackPanel content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center }; content.Children.Add(new TextBlock { Text = IconFor(item.Type), FontSize = 38, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush(Color.FromRgb(30, 145, 210)) }); content.Children.Add(new TextBlock { Text = item.Name, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap }); Button tile = new Button { Content = content, Tag = item, Style = (Style)FindResource("TileStyle") }; tile.Click += SelectItem; itemsPanel.Children.Add(tile); }
            ClearDetails();
        }
        private void SelectItem(object sender, RoutedEventArgs e) { selectedItem = (LaunchItem)((Button)sender).Tag; detailName.Text = selectedItem.Name; detailMemo.Text = selectedItem.Target; detailIcon.Text = IconFor(selectedItem.Type); typeText.Text = selectedItem.Type.ToString(); targetText.Text = selectedItem.Target; argumentsText.Text = selectedItem.Arguments; workingDirectoryText.Text = selectedItem.WorkingDirectory; }
        private void LaunchSelected(object sender, RoutedEventArgs e) { if (selectedItem != null) TryLaunch(selectedItem); }
        private void LaunchAll(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; var failures = launcher.LaunchWorkspace(workspace); if (failures.Count > 0) MessageBox.Show(string.Join("\n", failures), "起動できなかった項目"); }
        private void TryLaunch(LaunchItem item) { try { launcher.Launch(item); } catch (Exception ex) { MessageBox.Show(ex.Message, "起動エラー"); } }
        private void OpenSelectedFolder(object sender, RoutedEventArgs e) { if (selectedItem == null || string.IsNullOrWhiteSpace(selectedItem.Target)) return; string path = Directory.Exists(selectedItem.Target) ? selectedItem.Target : Path.GetDirectoryName(selectedItem.Target); if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"")); }
        private void OpenSettings(object sender, RoutedEventArgs e) { StructureSettingsEnvironmental settings = new StructureSettingsEnvironmental(); new EnvironmentWindow(ref settings).ShowDialog(); }
        private void AddItem(object sender, RoutedEventArgs e) { MessageBox.Show("LaunchItemの追加編集は次フェーズで実装します。workspaces.jsonは手動編集できます。", "追加"); }
        private void EditItem(object sender, RoutedEventArgs e) { MessageBox.Show("LaunchItemの編集は次フェーズで実装します。workspaces.jsonは手動編集できます。", "編集"); }
        private void ToggleLayoutBlock(object sender, RoutedEventArgs e) { Button block = (Button)sender; block.Opacity = block.Opacity == 1.0 ? 0.35 : 1.0; }
        private static string IconFor(LaunchItemType type) { switch (type) { case LaunchItemType.Folder: return "▰"; case LaunchItemType.Url: return "◎"; case LaunchItemType.Command: return ">_"; case LaunchItemType.File: return "▤"; default: return "◆"; } }
        private void ClearDetails() { selectedItem = null; detailName.Text = "LaunchItemを選択"; detailMemo.Text = detailIcon.Text = typeText.Text = targetText.Text = argumentsText.Text = workingDirectoryText.Text = ""; }
    }
}
