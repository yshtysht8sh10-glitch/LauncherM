using System.IO;
using System.Windows;
using System.Windows.Controls;
using LauncherM.Application;
using LauncherM.Domain;
using LauncherM.Infrastructure;

namespace LauncherM
{
    public partial class WorkspaceWindow : Window
    {
        private WorkspaceDocument document; private readonly LaunchService launcher = new LaunchService();
        public WorkspaceWindow() { InitializeComponent(); string basePath = Directory.GetCurrentDirectory(); document = new WorkspaceRepository(Path.Combine(basePath, "workspaces.json")).LoadOrMigrate(Path.Combine(basePath, "de.txt")); workspaceBox.ItemsSource = document.Workspaces; if (document.Workspaces.Count > 0) workspaceBox.SelectedIndex = 0; }
        private void WorkspaceChanged(object sender, SelectionChangedEventArgs e) { itemsPanel.Children.Clear(); Workspace w = workspaceBox.SelectedItem as Workspace; if (w == null) return; foreach (LaunchItem item in w.LaunchItems) { Button b = new Button { Content = item.Name, Tag = item, Margin = new Thickness(0, 2, 0, 2), Height = 30 }; b.Click += LaunchOne; itemsPanel.Children.Add(b); } }
        private void LaunchOne(object sender, RoutedEventArgs e) { try { launcher.Launch((LaunchItem)((Button)sender).Tag); } catch (System.Exception ex) { MessageBox.Show(ex.Message, "起動エラー"); } }
        private void LaunchAll(object sender, RoutedEventArgs e) { Workspace w = workspaceBox.SelectedItem as Workspace; if (w == null) return; var failures = launcher.LaunchWorkspace(w); if (failures.Count > 0) MessageBox.Show(string.Join("\n", failures), "起動できなかった項目"); }
    }
}
