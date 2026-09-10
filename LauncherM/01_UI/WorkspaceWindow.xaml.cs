using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using LauncherM.Application;
using LauncherM.Domain;
using LauncherM.Infrastructure;

namespace LauncherM
{
    public partial class WorkspaceWindow : Window
    {
        private readonly WorkspaceDocument document;
        private readonly WorkspaceRepository repository;
        private readonly LaunchService launcher = new LaunchService();
        private readonly GeneralPurpose generalPurpose = new GeneralPurpose();
        private readonly WebsiteIconCache websiteIcons = new WebsiteIconCache();
        private LaunchItem selectedItem;
        private readonly HashSet<LaunchItem> selectedItems = new HashSet<LaunchItem>();
        private readonly Dictionary<LaunchItem, Button> itemTiles = new Dictionary<LaunchItem, Button>();
        private Point dragStart;
        public WorkspaceWindow()
        {
            InitializeComponent();
            workingDirectoryText.Visibility = Visibility.Collapsed;
            Loaded += (s, e) => HideWorkingDirectoryField(this);
            itemsDropArea.PreviewMouseLeftButtonDown += ItemsAreaMouseDown;
            itemsDropArea.PreviewMouseMove += ItemsAreaMouseMove;
            itemsDropArea.PreviewMouseLeftButtonUp += ItemsAreaMouseUp;
            Loaded += (s, e) => { Button workspaceMenu = FindButtonByContent(this, "…"); if (workspaceMenu != null) workspaceMenu.Click += WorkspaceMenu; };
            string basePath = Directory.GetCurrentDirectory();
            repository = new WorkspaceRepository(Path.Combine(basePath, "workspaces.json"));
            document = repository.LoadOrMigrate(Path.Combine(basePath, "de.txt"));
            workspaceBox.ItemsSource = document.Workspaces;
            workspaceBox.IsEditable = true;
            workspaceBox.IsReadOnly = true;
            if (document.Workspaces.Count > 0) workspaceBox.SelectedIndex = 0;
            Loaded += async (s, e) => await LoadMissingWebsiteIcons();
        }
        private void WorkspaceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (updatingWorkspaceDisplay) return;
            if (showAllWorkspaces) { showAllWorkspaces = false; workspaceBox.Text = workspaceBox.SelectedItem is Workspace selectedWorkspace ? selectedWorkspace.Name : ""; }
            itemsPanel.Children.Clear(); itemTiles.Clear(); selectedItems.Clear(); Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return;
            foreach (LaunchItem item in workspace.LaunchItems) { StackPanel content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center }; Image image = new Image { Source = GetItemIcon(item), Width = 42, Height = 42, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center }; content.Children.Add(image); content.Children.Add(new TextBlock { Text = item.Name, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap }); Button tile = new Button { Content = content, Tag = item, Style = (Style)FindResource("TileStyle") }; tile.Click += SelectItem; tile.MouseDoubleClick += OpenItemByDoubleClick; tile.MouseRightButtonDown += SelectItemForContextMenu; tile.ContextMenu = CreateItemContextMenu(); itemTiles[item] = tile; itemsPanel.Children.Add(tile); }
            ClearDetails();
        }
        private void SelectItem(object sender, RoutedEventArgs e) { Button tile = (Button)sender; LaunchItem item = (LaunchItem)tile.Tag; if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) { if (!selectedItems.Add(item)) selectedItems.Remove(item); } else { selectedItems.Clear(); selectedItems.Add(item); } selectedItem = item; UpdateSelectionVisuals(); detailName.Text = selectedItem.Name; detailMemo.Text = selectedItem.Target; detailIcon.Source = GetItemIcon(selectedItem); typeText.Text = selectedItem.Type.ToString(); targetText.Text = selectedItem.Target; argumentsText.Text = selectedItem.Arguments; workingDirectoryText.Text = selectedItem.WorkingDirectory; e.Handled = true; }
        private void SelectItemForContextMenu(object sender, MouseButtonEventArgs e) { Button tile = (Button)sender; LaunchItem item = (LaunchItem)tile.Tag; if (!selectedItems.Contains(item)) { selectedItems.Clear(); selectedItems.Add(item); selectedItem = item; UpdateSelectionVisuals(); } }
        private void OpenItemByDoubleClick(object sender, MouseButtonEventArgs e) { LaunchItem item = (LaunchItem)((Button)sender).Tag; selectedItem = item; TryLaunch(item); e.Handled = true; }
        private ContextMenu CreateItemContextMenu() { ContextMenu menu = new ContextMenu(); MenuItem delete = new MenuItem { Header = "削除" }; delete.Click += DeleteSelectedItems; MenuItem icon = new MenuItem { Header = "アイコンを変更" }; icon.Click += ChangeSelectedIcon; menu.Items.Add(delete); menu.Items.Add(icon); return menu; }
        private void DeleteSelectedItems(object sender, RoutedEventArgs e) { if (selectedItems.Count == 0) return; foreach (Workspace workspace in document.Workspaces) foreach (LaunchItem item in new List<LaunchItem>(selectedItems)) workspace.LaunchItems.Remove(item); repository.Save(document); if (showAllWorkspaces) RenderItems(); else WorkspaceChanged(null, null); }
        private void ChangeSelectedIcon(object sender, RoutedEventArgs e) { if (selectedItem == null) return; using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog { Title = "アイコンに使用するファイルを選択", Filter = "実行ファイル・ショートカット・画像|*.exe;*.lnk;*.ico;*.png;*.jpg;*.jpeg|すべてのファイル|*.*" }) { if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return; selectedItem.IconPath = dialog.FileName; repository.Save(document); RenderItems(); } }
        private void WorkspaceMenu(object sender, RoutedEventArgs e) { ContextMenu menu = new ContextMenu(); AddMenuItem(menu, "Workspaceを追加", AddWorkspace); AddMenuItem(menu, "Workspace名を変更", RenameWorkspace); AddMenuItem(menu, "Workspaceを削除", DeleteWorkspace); AddMenuItem(menu, showAllWorkspaces ? "選択Workspaceのみ表示" : "全Workspaceを表示", ToggleShowAllWorkspaces); menu.IsOpen = true; }
        private bool showAllWorkspaces;
        private bool updatingWorkspaceDisplay;
        private static void AddMenuItem(ContextMenu menu, string header, RoutedEventHandler handler) { MenuItem item = new MenuItem { Header = header }; item.Click += handler; menu.Items.Add(item); }
        private void AddWorkspace(object sender, RoutedEventArgs e) { string name = Prompt("Workspace名", "新しいWorkspace"); if (string.IsNullOrWhiteSpace(name)) return; document.Workspaces.Add(new Workspace { Name = name.Trim() }); repository.Save(document); workspaceBox.Items.Refresh(); workspaceBox.SelectedIndex = document.Workspaces.Count - 1; }
        private void RenameWorkspace(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; string name = Prompt("Workspace名を変更", workspace.Name); if (string.IsNullOrWhiteSpace(name)) return; workspace.Name = name.Trim(); repository.Save(document); workspaceBox.Items.Refresh(); updatingWorkspaceDisplay = true; workspaceBox.Text = workspace.Name; updatingWorkspaceDisplay = false; }
        private void DeleteWorkspace(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null || document.Workspaces.Count <= 1) return; if (MessageBox.Show("選択中のWorkspaceを削除しますか？", "確認", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; int index = workspaceBox.SelectedIndex; document.Workspaces.Remove(workspace); repository.Save(document); workspaceBox.Items.Refresh(); workspaceBox.SelectedIndex = Math.Min(index, document.Workspaces.Count - 1); }
        private void ToggleShowAllWorkspaces(object sender, RoutedEventArgs e) { showAllWorkspaces = !showAllWorkspaces; updatingWorkspaceDisplay = true; workspaceBox.Text = showAllWorkspaces ? "全Workspace" : (workspaceBox.SelectedItem is Workspace workspace ? workspace.Name : ""); updatingWorkspaceDisplay = false; RenderItems(); }
        private void RenderItems() { itemsPanel.Children.Clear(); itemTiles.Clear(); selectedItems.Clear(); if (showAllWorkspaces) { foreach (Workspace workspace in document.Workspaces) foreach (LaunchItem item in workspace.LaunchItems) AddTile(item); } else { WorkspaceChanged(null, null); } }
        private void AddTile(LaunchItem item) { StackPanel content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center }; content.Children.Add(new Image { Source = GetItemIcon(item), Width = 42, Height = 42, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center }); content.Children.Add(new TextBlock { Text = item.Name, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap }); Button tile = new Button { Content = content, Tag = item, Style = (Style)FindResource("TileStyle") }; tile.Click += SelectItem; tile.MouseDoubleClick += OpenItemByDoubleClick; tile.MouseRightButtonDown += SelectItemForContextMenu; tile.ContextMenu = CreateItemContextMenu(); itemTiles[item] = tile; itemsPanel.Children.Add(tile); }
        private static Button FindButtonByContent(DependencyObject root, string content) { for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) { DependencyObject child = VisualTreeHelper.GetChild(root, i); Button button = child as Button; if (button != null && button.Content as string == content) return button; Button found = FindButtonByContent(child, content); if (found != null) return found; } return null; }
        private static void HideWorkingDirectoryField(DependencyObject root) { for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) { DependencyObject child = VisualTreeHelper.GetChild(root, i); TextBlock label = child as TextBlock; if (label != null && label.Text == "作業フォルダ") label.Visibility = Visibility.Collapsed; if (child == null) continue; TextBox box = child as TextBox; if (box != null && box.Name == "workingDirectoryText") box.Visibility = Visibility.Collapsed; HideWorkingDirectoryField(child); } }
        private static string Prompt(string title, string initial) { Window dialog = new Window { Title = title, Width = 360, Height = 130, WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize }; StackPanel panel = new StackPanel { Margin = new Thickness(12) }; TextBox input = new TextBox { Text = initial, Margin = new Thickness(0, 0, 0, 10) }; Button ok = new Button { Content = "OK", IsDefault = true, Width = 70, HorizontalAlignment = HorizontalAlignment.Right }; ok.Click += (s, e) => dialog.DialogResult = true; panel.Children.Add(input); panel.Children.Add(ok); dialog.Content = panel; return dialog.ShowDialog() == true ? input.Text : null; }
        private void WindowPreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Delete && selectedItems.Count > 0) { DeleteSelectedItems(null, null); e.Handled = true; } }
        private void ItemsAreaMouseDown(object sender, MouseButtonEventArgs e) { dragStart = e.GetPosition(itemsDropArea); if (FindTileAt(dragStart) == null) { selectedItems.Clear(); ClearDetails(); UpdateSelectionVisuals(); } }
        private void ItemsAreaMouseMove(object sender, MouseEventArgs e) { if (e.LeftButton != MouseButtonState.Pressed) return; Point point = e.GetPosition(itemsDropArea); if ((point - dragStart).Length < 8) return; Button hit = FindTileAt(point); if (hit != null) { LaunchItem item = (LaunchItem)hit.Tag; selectedItems.Add(item); selectedItem = item; UpdateSelectionVisuals(); } }
        private void ItemsAreaMouseUp(object sender, MouseButtonEventArgs e) { }
        private Button FindTileAt(Point point) { foreach (Button tile in itemTiles.Values) { Point topLeft = tile.TranslatePoint(new Point(0, 0), itemsDropArea); if (new Rect(topLeft, tile.RenderSize).Contains(point)) return tile; } return null; }
        private void UpdateSelectionVisuals() { foreach (KeyValuePair<LaunchItem, Button> pair in itemTiles) { bool isActive = selectedItem == pair.Key; bool isSelected = selectedItems.Contains(pair.Key); pair.Value.BorderBrush = isActive ? new SolidColorBrush(Color.FromRgb(255, 180, 45)) : (isSelected ? new SolidColorBrush(Color.FromRgb(40, 170, 255)) : new SolidColorBrush(Color.FromRgb(80, 80, 80))); pair.Value.BorderThickness = isActive ? new Thickness(4) : (isSelected ? new Thickness(3) : new Thickness(1)); pair.Value.Background = isActive ? new SolidColorBrush(Color.FromRgb(105, 78, 35)) : (isSelected ? new SolidColorBrush(Color.FromRgb(55, 75, 95)) : new SolidColorBrush(Color.FromRgb(58, 58, 58))); } }
        private void LaunchSelected(object sender, RoutedEventArgs e) { if (selectedItem != null) TryLaunch(selectedItem); }
        private void LaunchAll(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; var failures = launcher.LaunchWorkspace(workspace); if (failures.Count > 0) MessageBox.Show(string.Join("\n", failures), "起動できなかった項目"); }
        private void TryLaunch(LaunchItem item) { try { launcher.Launch(item); } catch (Exception ex) { MessageBox.Show(ex.Message, "起動エラー"); } }
        private void OpenSelectedFolder(object sender, RoutedEventArgs e) { if (selectedItem == null || string.IsNullOrWhiteSpace(selectedItem.Target)) return; string path = Directory.Exists(selectedItem.Target) ? selectedItem.Target : Path.GetDirectoryName(selectedItem.Target); if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"")); }
        private void OpenSettings(object sender, RoutedEventArgs e) { StructureSettingsEnvironmental settings = new StructureSettingsEnvironmental(); new EnvironmentWindow(ref settings).ShowDialog(); }
        private async void AddItem(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; LaunchItem item = ShowLaunchItemDialog(null); if (item == null) return; workspace.LaunchItems.Add(item); await EnsureWebsiteIcon(item); repository.Save(document); WorkspaceChanged(null, null); }
        private async void EditItem(object sender, RoutedEventArgs e) { if (selectedItem == null) return; LaunchItem edited = ShowLaunchItemDialog(selectedItem); if (edited == null) return; selectedItem.Name = edited.Name; selectedItem.Type = edited.Type; selectedItem.Target = edited.Target; selectedItem.Arguments = edited.Arguments; selectedItem.WorkingDirectory = edited.WorkingDirectory; selectedItem.Browser = edited.Browser; selectedItem.BrowserProfile = edited.BrowserProfile; selectedItem.BrowserWindowGroup = edited.BrowserWindowGroup; selectedItem.IconPath = edited.IconPath; await EnsureWebsiteIcon(selectedItem); repository.Save(document); RenderItems(); }
        private LaunchItem ShowLaunchItemDialog(LaunchItem source)
        {
            Window dialog = new Window { Title = source == null ? "LaunchItemを追加" : "LaunchItemを編集", Width = 520, SizeToContent = SizeToContent.Height, WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize, Owner = this };
            StackPanel panel = new StackPanel { Margin = new Thickness(14) }; TextBox name = AddDialogText(panel, "名前", source == null ? "" : source.Name); ComboBox type = new ComboBox { ItemsSource = Enum.GetValues(typeof(LaunchItemType)), SelectedItem = source == null ? (object)LaunchItemType.Application : source.Type, Margin = new Thickness(0, 2, 0, 8) }; panel.Children.Add(new TextBlock { Text = "種類" }); panel.Children.Add(type); TextBox target = AddDialogText(panel, "実行ファイル / Target", source == null ? "" : source.Target); TextBox arguments = AddDialogText(panel, "引数", source == null ? "" : source.Arguments); ComboBox browser = new ComboBox { ItemsSource = new[] { "Default", "Chrome", "Edge", "Firefox" }, SelectedItem = source == null || string.IsNullOrWhiteSpace(source.Browser) ? "Default" : source.Browser, Margin = new Thickness(0, 2, 0, 8) }; panel.Children.Add(new TextBlock { Text = "ブラウザ（Urlのみ）" }); panel.Children.Add(browser); var urlPanel = new StackPanel(); panel.Children.Add(urlPanel);
            urlPanel.Children.Add(new TextBlock { Text = "プロファイル（候補を選択／内部名を手入力）" });
            var profile = new ComboBox { IsEditable = true, DisplayMemberPath = "Label", Margin = new Thickness(0, 2, 0, 8) }; urlPanel.Children.Add(profile);
            var profileHint = new TextBlock { TextWrapping = TextWrapping.Wrap }; urlPanel.Children.Add(profileHint);
            Func<string> profileId = () => profile.SelectedItem is BrowserProfileChoice choice && profile.Text == choice.Label ? choice.Directory : profile.Text.Trim();
            Action<string> loadProfiles = saved => {
                string message;
                var choices = BrowserProfiles.Detect(browser.SelectedItem as string, out message);
                choices.Insert(0, new BrowserProfileChoice { Directory = "", DisplayName = "未指定" });
                profile.ItemsSource = choices;
                profile.SelectedItem = choices.Find(candidate => candidate.Directory == (saved ?? ""));
                if (profile.SelectedItem == null) profile.Text = saved ?? "";
                profileHint.Text = message;
            };
            urlPanel.Children.Add(new TextBlock { Text = "Window（同じ名前＝同じWindowの別Tab）" });
            var windowGroup = new ComboBox { IsEditable = true, Margin = new Thickness(0, 2, 0, 8) };
            windowGroup.Items.Add(""); windowGroup.Items.Add("Window 1"); windowGroup.Items.Add("Window 2"); windowGroup.Items.Add("Window 3");
            var ownerWorkspace = document.Workspaces.Find(w => source != null && w.LaunchItems.Contains(source)) ?? workspaceBox.SelectedItem as Workspace;
            if (ownerWorkspace != null) foreach (var member in ownerWorkspace.LaunchItems)
                if (!string.IsNullOrWhiteSpace(member.BrowserWindowGroup) && !windowGroup.Items.Contains(member.BrowserWindowGroup)) windowGroup.Items.Add(member.BrowserWindowGroup);
            windowGroup.Text = source?.BrowserWindowGroup ?? ""; urlPanel.Children.Add(windowGroup);
            var hint = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) }; urlPanel.Children.Add(hint);
            Action updateUrlFields = () => {
                bool visible = (LaunchItemType)type.SelectedItem == LaunchItemType.Url;
                browser.Visibility = urlPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                ((TextBlock)panel.Children[panel.Children.IndexOf(browser) - 1]).Visibility = browser.Visibility;
                string selectedBrowser = browser.SelectedItem as string;
                bool grouped = selectedBrowser == "Chrome" || selectedBrowser == "Edge";
                windowGroup.IsEnabled = grouped; profile.IsEnabled = selectedBrowser != "Default";
                hint.Text = grouped ? "Window空欄は従来の起動。一括起動では指定Windowごとに新しいウィンドウを開きます。ログイン状態はブラウザ側で準備してください。" : "Default / FirefoxではWindow構成を制御しません。Firefoxはプロファイル名を手入力。Defaultはプロファイル指定不可。";
            };
            browser.SelectionChanged += (s, e) => { loadProfiles(""); updateUrlFields(); };
            type.SelectionChanged += (s, e) => updateUrlFields();
            loadProfiles(source?.BrowserProfile); updateUrlFields();
            panel.Children.Add(new TextBlock { Text = "アイコン（空欄のURLはfaviconを自動取得）" });
            var iconRow = new DockPanel { Margin = new Thickness(0, 2, 0, 10) };
            var iconBrowse = new Button { Content = "参照…", Width = 75, Margin = new Thickness(6, 0, 0, 0) };
            DockPanel.SetDock(iconBrowse, Dock.Right); iconRow.Children.Add(iconBrowse);
            var iconPath = new TextBox { Text = source?.IconPath ?? "", Margin = new Thickness(0) }; iconRow.Children.Add(iconPath); panel.Children.Add(iconRow);
            iconBrowse.Click += (s, e) => { using (var picker = new System.Windows.Forms.OpenFileDialog { Title = "アイコンに使用するファイルを選択", Filter = "アイコン・画像・実行ファイル|*.ico;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.exe;*.lnk|すべてのファイル|*.*" }) if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK) iconPath.Text = picker.FileName; };
            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right }; Button ok = new Button { Content = "OK", Width = 75, IsDefault = true }; Button cancel = new Button { Content = "キャンセル", Width = 90, IsCancel = true, Margin = new Thickness(8, 0, 0, 0) }; buttons.Children.Add(ok); buttons.Children.Add(cancel); panel.Children.Add(buttons); dialog.Content = panel; ok.Click += (s, e) => { if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(target.Text)) { MessageBox.Show("名前とTargetを入力してください。", "入力確認"); return; } if ((LaunchItemType)type.SelectedItem == LaunchItemType.Url && !Uri.IsWellFormedUriString(target.Text.Trim(), UriKind.Absolute)) { MessageBox.Show("有効なURLを入力してください。", "入力確認"); return; } if (!string.IsNullOrWhiteSpace(iconPath.Text) && !File.Exists(iconPath.Text.Trim())) { MessageBox.Show("指定したアイコンファイルが見つかりません。", "入力確認"); return; } dialog.DialogResult = true; }; if (dialog.ShowDialog() != true) return null; string savedIcon = iconPath.Text.Trim(); if (source != null && websiteIcons.IsManagedPath(savedIcon) && !string.Equals(source.Target, target.Text.Trim(), StringComparison.OrdinalIgnoreCase)) savedIcon = ""; return new LaunchItem { Id = source == null ? Guid.NewGuid() : source.Id, Name = name.Text.Trim(), Type = (LaunchItemType)type.SelectedItem, Target = target.Text.Trim(), Arguments = arguments.Text.Trim(), WorkingDirectory = source == null ? "" : source.WorkingDirectory, Browser = browser.SelectedItem == null ? "Default" : browser.SelectedItem.ToString(), BrowserProfile = profileId(), BrowserWindowGroup = windowGroup.Text.Trim(), IconPath = savedIcon };
        }
        private static TextBox AddDialogText(StackPanel panel, string label, string value) { panel.Children.Add(new TextBlock { Text = label }); TextBox box = new TextBox { Text = value }; panel.Children.Add(box); return box; }
        private void ToggleLayoutBlock(object sender, RoutedEventArgs e) { Button block = (Button)sender; block.Opacity = block.Opacity == 1.0 ? 0.35 : 1.0; }
        private void ItemsDropAreaDragOver(object sender, DragEventArgs e) { bool isFileDrop = e.Data.GetDataPresent(DataFormats.FileDrop); string text = e.Data.GetDataPresent(DataFormats.StringFormat) ? e.Data.GetData(DataFormats.StringFormat) as string : null; bool isUrl = Uri.IsWellFormedUriString(text, UriKind.Absolute) && (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)); e.Effects = isFileDrop || isUrl ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true; }
        private async void ItemsDropAreaDrop(object sender, DragEventArgs e)
        {
            Workspace workspace = workspaceBox.SelectedItem as Workspace;
            string[] paths = e.Data.GetDataPresent(DataFormats.FileDrop) ? (string[])e.Data.GetData(DataFormats.FileDrop) : null;
            string droppedText = e.Data.GetDataPresent(DataFormats.StringFormat) ? e.Data.GetData(DataFormats.StringFormat) as string : null;
            if (workspace == null) return;
            if (paths == null && Uri.IsWellFormedUriString(droppedText, UriKind.Absolute) && (droppedText.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || droppedText.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                var urlItem = new LaunchItem { Name = droppedText, Type = LaunchItemType.Url, Target = droppedText, Arguments = "", WorkingDirectory = "" }; workspace.LaunchItems.Add(urlItem); await EnsureWebsiteIcon(urlItem); repository.Save(document); WorkspaceChanged(null, null); return;
            }
            if (paths == null) return;
            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) && !Directory.Exists(path)) continue;
                bool isDirectory = Directory.Exists(path);
                LaunchItemType type = isDirectory ? LaunchItemType.Folder : (Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".lnk", StringComparison.OrdinalIgnoreCase) ? LaunchItemType.Application : LaunchItemType.File);
                workspace.LaunchItems.Add(new LaunchItem { Name = Path.GetFileNameWithoutExtension(path), Type = type, Target = path, WorkingDirectory = isDirectory ? path : Path.GetDirectoryName(path) });
            }
            repository.Save(document);
            WorkspaceChanged(null, null);
        }
        private static string IconFor(LaunchItemType type) { switch (type) { case LaunchItemType.Folder: return "▰"; case LaunchItemType.Url: return "◎"; case LaunchItemType.Command: return ">_"; case LaunchItemType.File: return "▤"; default: return "◆"; } }
        private BitmapSource GetItemIcon(LaunchItem item) { try { string iconPath = !string.IsNullOrWhiteSpace(item.IconPath) ? item.IconPath : item.Target; if (File.Exists(iconPath)) { string ext = Path.GetExtension(iconPath).ToLowerInvariant(); if (websiteIcons.IsManagedPath(iconPath) || ext == ".ico" || ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif") { var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.UriSource = new Uri(iconPath, UriKind.Absolute); image.EndInit(); image.Freeze(); return image; } return generalPurpose.GetIconFromFilePathSpecified(iconPath); } if (Directory.Exists(iconPath)) return generalPurpose.GetIconFromFilePathSpecified(iconPath); } catch { } return null; }
        private async Task EnsureWebsiteIcon(LaunchItem item) { if (item.Type != LaunchItemType.Url || !string.IsNullOrWhiteSpace(item.IconPath)) return; string path = await Task.Run(() => websiteIcons.GetOrDownload(item.Target)); if (!string.IsNullOrWhiteSpace(path)) item.IconPath = path; }
        private async Task LoadMissingWebsiteIcons() { bool changed = false; foreach (var workspace in document.Workspaces) foreach (var item in workspace.LaunchItems) if (item.Type == LaunchItemType.Url && string.IsNullOrWhiteSpace(item.IconPath)) { await EnsureWebsiteIcon(item); changed |= !string.IsNullOrWhiteSpace(item.IconPath); } if (changed) { repository.Save(document); RenderItems(); } }
        private void ClearDetails() { selectedItem = null; detailName.Text = "LaunchItemを選択"; detailMemo.Text = typeText.Text = targetText.Text = argumentsText.Text = workingDirectoryText.Text = ""; detailIcon.Source = null; }
    }
}
