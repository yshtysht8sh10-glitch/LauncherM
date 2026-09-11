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
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Data;
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
        private readonly WorkspaceSessionManager sessions = new WorkspaceSessionManager();
        private LaunchItem selectedItem;
        private readonly HashSet<LaunchItem> selectedItems = new HashSet<LaunchItem>();
        private readonly Dictionary<LaunchItem, Button> itemTiles = new Dictionary<LaunchItem, Button>();
        private Point dragStart;
        private Point workspaceTabDragStart;
        private Workspace draggedWorkspace;
        private Point itemDragStart;
        private LaunchItem draggedItem;
        private DragPreviewAdorner dragPreview;
        private AdornerLayer dragPreviewLayer;
        private FrameworkElement dragSourceElement;
        private FrameworkElement reorderHintElement;
        private double reorderHintOffset;
        private Workspace inlineRenameWorkspace;
        private bool cancellingInlineRename;
        private readonly Dictionary<FrameworkElement, double> originalFontSizes = new Dictionary<FrameworkElement, double>();
        private Color accentColor = Color.FromRgb(101, 181, 255);
        private double cardScale = 1;
        private double workspacePaneWidthBeforeOverview = 220;
        public WorkspaceWindow()
        {
            InitializeComponent();
            itemsDropArea.PreviewMouseLeftButtonDown += ItemsAreaMouseDown;
            itemsDropArea.PreviewMouseMove += ItemsAreaMouseMove;
            itemsDropArea.PreviewMouseLeftButtonUp += ItemsAreaMouseUp;
            string basePath = Directory.GetCurrentDirectory();
            repository = new WorkspaceRepository(Path.Combine(basePath, "workspaces.json"));
            document = repository.LoadOrMigrate(Path.Combine(basePath, "de.txt"));
            cardScale = Properties.Settings.Default.CardScale; if (cardScale < .6 || cardScale > 1.8) cardScale = 1;
            workspaceBox.ItemsSource = document.Workspaces;
            workspaceBox.IsEditable = true;
            workspaceBox.IsReadOnly = true;
            RenderWorkspaceTabs();
            RestoreSplitterWidths();
            if (document.Workspaces.Count > 0) workspaceBox.SelectedIndex = 0;
            ApplyAppearance();
            Loaded += async (s, e) => await LoadMissingWebsiteIcons();
        }
        private void WorkspaceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (updatingWorkspaceDisplay) return;
            if (showAllWorkspaces) { UpdateWorkspaceTabs(); RenderAllWorkspaces(); return; }
            itemsPanel.Children.Clear(); itemTiles.Clear(); selectedItems.Clear(); Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return;
            workspaceNameText.Text = workspace.Name;
            UpdateWorkspaceTabs();
            foreach (LaunchItem item in workspace.LaunchItems) AddTile(item);
            ClearDetails();
            ApplyAppearance();
        }
        private void SelectItem(object sender, RoutedEventArgs e) { Button tile = (Button)sender; LaunchItem item = (LaunchItem)tile.Tag; if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) { if (!selectedItems.Add(item)) selectedItems.Remove(item); } else { selectedItems.Clear(); selectedItems.Add(item); } selectedItem = item; UpdateSelectionVisuals(); e.Handled = true; }
        private void SelectItemForContextMenu(object sender, MouseButtonEventArgs e) { Button tile = (Button)sender; LaunchItem item = (LaunchItem)tile.Tag; if (!selectedItems.Contains(item)) { selectedItems.Clear(); selectedItems.Add(item); selectedItem = item; UpdateSelectionVisuals(); } }
        private void ItemTileMouseDown(object sender, MouseButtonEventArgs e) { draggedItem = ((Button)sender).Tag as LaunchItem; itemDragStart = e.GetPosition(itemsPanel); }
        private void ItemTileMouseMove(object sender, MouseEventArgs e)
        {
            if (showAllWorkspaces || e.LeftButton != MouseButtonState.Pressed || draggedItem == null) return;
            Point current = e.GetPosition(itemsPanel);
            if (Math.Abs(current.X - itemDragStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(current.Y - itemDragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            LaunchItem source = draggedItem; draggedItem = null;
            BeginDragVisual(itemsDropArea, (FrameworkElement)sender);
            UpdateDragVisual(e.GetPosition(itemsDropArea));
            try { DragDrop.DoDragDrop((DependencyObject)sender, new DataObject(typeof(LaunchItem), source), DragDropEffects.Move); }
            finally { EndDragVisual(); }
        }
        private void ItemTileDragOver(object sender, DragEventArgs e) { if (!e.Data.GetDataPresent(typeof(LaunchItem))) return; e.Effects = !showAllWorkspaces ? DragDropEffects.Move : DragDropEffects.None; Button target = (Button)sender; Point point = e.GetPosition(target); ShowReorderHint(target, point.X >= target.ActualWidth / 2); UpdateDragVisual(e.GetPosition(itemsDropArea)); e.Handled = true; }
        private void ItemTileDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(LaunchItem))) return;
            LaunchItem source = e.Data.GetData(typeof(LaunchItem)) as LaunchItem; LaunchItem target = ((Button)sender).Tag as LaunchItem;
            if (source == null || target == null || ReferenceEquals(source, target)) { e.Handled = true; return; }
            Button targetTile = (Button)sender; bool insertAfter = e.GetPosition(targetTile).X >= targetTile.ActualWidth / 2;
            MoveLaunchItem(source, target, insertAfter); e.Handled = true;
        }
        private void ItemsPanelDragOver(object sender, DragEventArgs e) { if (!e.Data.GetDataPresent(typeof(LaunchItem))) return; e.Effects = !showAllWorkspaces ? DragDropEffects.Move : DragDropEffects.None; ClearReorderHint(); UpdateDragVisual(e.GetPosition(itemsDropArea)); e.Handled = true; }
        private void ItemsPanelDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(LaunchItem))) return;
            LaunchItem source = e.Data.GetData(typeof(LaunchItem)) as LaunchItem; Workspace workspace = workspaceBox.SelectedItem as Workspace;
            if (source == null || workspace == null || !workspace.LaunchItems.Remove(source)) return;
            workspace.LaunchItems.Add(source); SaveLaunchItemOrder(workspace, source); e.Handled = true;
        }
        private void MoveLaunchItem(LaunchItem source, LaunchItem target, bool insertAfter)
        {
            Workspace workspace = workspaceBox.SelectedItem as Workspace;
            if (workspace == null || !workspace.LaunchItems.Remove(source)) return;
            int targetIndex = workspace.LaunchItems.IndexOf(target);
            workspace.LaunchItems.Insert(targetIndex + (insertAfter ? 1 : 0), source);
            SaveLaunchItemOrder(workspace, source);
        }
        private void SaveLaunchItemOrder(Workspace workspace, LaunchItem activeItem)
        {
            repository.Save(document); WorkspaceChanged(null, null); selectedItem = activeItem; selectedItems.Add(activeItem); UpdateSelectionVisuals();
        }
        private void OpenItemByDoubleClick(object sender, MouseButtonEventArgs e) { LaunchItem item = (LaunchItem)((Button)sender).Tag; selectedItem = item; TryLaunch(item); e.Handled = true; }
        private ContextMenu CreateItemContextMenu() { ContextMenu menu = new ContextMenu(); MenuItem delete = new MenuItem { Header = "削除" }; delete.Click += DeleteSelectedItems; MenuItem icon = new MenuItem { Header = "アイコンを変更" }; icon.Click += ChangeSelectedIcon; menu.Items.Add(delete); menu.Items.Add(icon); return menu; }
        private void DeleteSelectedItems(object sender, RoutedEventArgs e) { if (selectedItems.Count == 0) return; foreach (Workspace workspace in document.Workspaces) foreach (LaunchItem item in new List<LaunchItem>(selectedItems)) workspace.LaunchItems.Remove(item); repository.Save(document); if (showAllWorkspaces) RenderItems(); else WorkspaceChanged(null, null); }
        private void ChangeSelectedIcon(object sender, RoutedEventArgs e) { if (selectedItem == null) return; using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog { Title = "アイコンに使用するファイルを選択", Filter = "実行ファイル・ショートカット・画像|*.exe;*.lnk;*.ico;*.png;*.jpg;*.jpeg|すべてのファイル|*.*" }) { if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return; selectedItem.IconPath = dialog.FileName; repository.Save(document); RenderItems(); } }
        private void WorkspaceMenu(object sender, RoutedEventArgs e) { ContextMenu menu = new ContextMenu(); AddMenuItem(menu, "Workspaceを追加", AddWorkspace); AddMenuItem(menu, "Workspace名を変更", RenameWorkspace); AddMenuItem(menu, "Workspaceを削除", DeleteWorkspace); AddMenuItem(menu, showAllWorkspaces ? "選択Workspaceのみ表示" : "全Workspaceを表示", ToggleShowAllWorkspaces); menu.IsOpen = true; }
        private bool showAllWorkspaces;
        private bool updatingWorkspaceDisplay;
        private static void AddMenuItem(ContextMenu menu, string header, RoutedEventHandler handler) { MenuItem item = new MenuItem { Header = header }; item.Click += handler; menu.Items.Add(item); }
        private void AddWorkspace(object sender, RoutedEventArgs e) { string name = Prompt("Workspace名", "新しいWorkspace"); if (string.IsNullOrWhiteSpace(name)) return; document.Workspaces.Add(new Workspace { Name = name.Trim() }); repository.Save(document); workspaceBox.Items.Refresh(); RenderWorkspaceTabs(); workspaceBox.SelectedIndex = document.Workspaces.Count - 1; }
        private void RenameWorkspace(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; string name = Prompt("Workspace名を変更", workspace.Name); if (string.IsNullOrWhiteSpace(name)) return; SaveWorkspaceName(workspace, name); }
        private void BeginInlineWorkspaceRename(object sender, MouseButtonEventArgs e)
        {
            Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return;
            inlineRenameWorkspace = workspace; cancellingInlineRename = false; workspaceNameEditor.Text = workspace.Name;
            workspaceNameText.Visibility = Visibility.Collapsed; workspaceNameEditor.Visibility = Visibility.Visible;
            workspaceNameEditor.Focus(); workspaceNameEditor.SelectAll(); e.Handled = true;
        }
        private void InlineWorkspaceNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { FinishInlineWorkspaceRename(true); e.Handled = true; }
            else if (e.Key == Key.Escape) { cancellingInlineRename = true; FinishInlineWorkspaceRename(false); e.Handled = true; }
        }
        private void InlineWorkspaceNameLostFocus(object sender, KeyboardFocusChangedEventArgs e) { if (workspaceNameEditor.Visibility == Visibility.Visible) FinishInlineWorkspaceRename(!cancellingInlineRename); }
        private void WindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (workspaceNameEditor.Visibility != Visibility.Visible) return;
            DependencyObject clicked = e.OriginalSource as DependencyObject;
            if (!ReferenceEquals(FindAncestor<TextBox>(clicked), workspaceNameEditor)) FinishInlineWorkspaceRename(true);
        }
        private void FinishInlineWorkspaceRename(bool save)
        {
            Workspace workspace = inlineRenameWorkspace; inlineRenameWorkspace = null;
            string name = workspaceNameEditor.Text;
            workspaceNameEditor.Visibility = Visibility.Collapsed; workspaceNameText.Visibility = Visibility.Visible;
            if (save && workspace != null && !string.IsNullOrWhiteSpace(name)) SaveWorkspaceName(workspace, name);
            else if (workspace != null && ReferenceEquals(workspaceBox.SelectedItem, workspace)) workspaceNameText.Text = workspace.Name;
            cancellingInlineRename = false;
        }
        private void SaveWorkspaceName(Workspace workspace, string name)
        {
            workspace.Name = name.Trim(); repository.Save(document); workspaceBox.Items.Refresh(); RenderWorkspaceTabs();
            updatingWorkspaceDisplay = true; workspaceBox.Text = workspace.Name; updatingWorkspaceDisplay = false;
            if (ReferenceEquals(workspaceBox.SelectedItem, workspace)) workspaceNameText.Text = workspace.Name;
            UpdateWorkspaceTabs();
        }
        private void DeleteWorkspace(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null || document.Workspaces.Count <= 1) return; if (sessions.ActiveCount(workspace.Id) > 0) { MessageBox.Show("このWorkspaceには起動中の追跡対象があります。先に「すべて閉じる」を実行してください。", "Workspaceを削除できません"); return; } if (MessageBox.Show("選択中のWorkspaceを削除しますか？", "確認", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; int index = workspaceBox.SelectedIndex; document.Workspaces.Remove(workspace); repository.Save(document); workspaceBox.Items.Refresh(); RenderWorkspaceTabs(); workspaceBox.SelectedIndex = Math.Min(index, document.Workspaces.Count - 1); }
        private void ToggleDisplayMode(object sender, RoutedEventArgs e) { showAllWorkspaces = !showAllWorkspaces; ApplyDisplayMode(); }
        private void ToggleShowAllWorkspaces(object sender, RoutedEventArgs e) { showAllWorkspaces = !showAllWorkspaces; ApplyDisplayMode(); }
        private void ApplyDisplayMode()
        {
            if (showAllWorkspaces)
            {
                if (workspaceColumn.ActualWidth >= 160) workspacePaneWidthBeforeOverview = workspaceColumn.ActualWidth;
                workspaceColumn.MinWidth = 0; workspaceColumn.Width = new GridLength(0); workspaceSplitterColumn.Width = new GridLength(0);
                workspacePane.Visibility = Visibility.Collapsed; workspaceSplitter.Visibility = Visibility.Collapsed; itemsToolbar.Visibility = Visibility.Collapsed;
                itemsPanel.Visibility = Visibility.Collapsed; allWorkspacesPanel.Visibility = Visibility.Visible; displayModeButton.Content = "▣ 現在のWorkspace表示";
                RenderAllWorkspaces();
            }
            else
            {
                workspaceColumn.MinWidth = 160; workspaceColumn.Width = new GridLength(Math.Max(160, workspacePaneWidthBeforeOverview)); workspaceSplitterColumn.Width = new GridLength(5);
                workspacePane.Visibility = Visibility.Visible; workspaceSplitter.Visibility = Visibility.Visible; itemsToolbar.Visibility = Visibility.Visible;
                itemsPanel.Visibility = Visibility.Visible; allWorkspacesPanel.Visibility = Visibility.Collapsed; displayModeButton.Content = "☷ 全Workspace表示";
                WorkspaceChanged(null, null);
            }
        }
        private void RenderItems() { if (showAllWorkspaces) RenderAllWorkspaces(); else WorkspaceChanged(null, null); }
        private void RenderAllWorkspaces()
        {
            allWorkspacesPanel.Children.Clear(); itemsPanel.Children.Clear(); itemTiles.Clear(); selectedItems.Clear(); ClearDetails();
            foreach (Workspace workspace in document.Workspaces)
            {
                var section = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
                var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
                var launch = new Button { Content = "▶", Tag = workspace, Width = 38, Height = 34, ToolTip = workspace.Name + " をすべて起動" }; launch.Click += LaunchWorkspaceSection;
                var close = new Button { Content = "■", Tag = workspace, Width = 38, Height = 34, Background = new SolidColorBrush(Color.FromRgb(89, 67, 67)), ToolTip = workspace.Name + " をすべて閉じる" }; close.Click += CloseWorkspaceSection;
                header.Children.Add(launch); header.Children.Add(close); header.Children.Add(new TextBlock { Text = workspace.Name, FontSize = 22, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(accentColor), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) });
                var cards = new WrapPanel(); cards.SetBinding(FrameworkElement.WidthProperty, new Binding("ViewportWidth") { Source = itemsScrollViewer }); foreach (LaunchItem item in workspace.LaunchItems) AddTile(item, cards);
                section.Children.Add(header); section.Children.Add(cards); section.Children.Add(new Separator { Margin = new Thickness(0, 8, 0, 0) }); allWorkspacesPanel.Children.Add(section);
            }
            ApplyAppearance();
        }
        private void AddTile(LaunchItem item, Panel host = null) { StackPanel content = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center }; content.Children.Add(new Image { Source = GetItemIcon(item), Width = 42 * cardScale, Height = 42 * cardScale, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center }); content.Children.Add(new TextBlock { Text = item.Name, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap }); Button tile = new Button { Content = content, Tag = item, Style = (Style)FindResource("TileStyle"), Width = 154 * cardScale, Height = 142 * cardScale, AllowDrop = true, ToolTip = showAllWorkspaces ? "ダブルクリックで起動" : "ドラッグして並び替え" }; tile.Click += SelectItem; tile.MouseDoubleClick += OpenItemByDoubleClick; tile.MouseRightButtonDown += SelectItemForContextMenu; tile.PreviewMouseLeftButtonDown += ItemTileMouseDown; tile.PreviewMouseMove += ItemTileMouseMove; tile.DragOver += ItemTileDragOver; tile.Drop += ItemTileDrop; tile.ContextMenu = CreateItemContextMenu(); itemTiles[item] = tile; (host ?? itemsPanel).Children.Add(tile); }
        private static Button FindButtonByContent(DependencyObject root, string content) { for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) { DependencyObject child = VisualTreeHelper.GetChild(root, i); Button button = child as Button; if (button != null && button.Content as string == content) return button; Button found = FindButtonByContent(child, content); if (found != null) return found; } return null; }
        private static string Prompt(string title, string initial) { Window dialog = new Window { Title = title, Width = 360, Height = 130, WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize }; StackPanel panel = new StackPanel { Margin = new Thickness(12) }; TextBox input = new TextBox { Text = initial, Margin = new Thickness(0, 0, 0, 10) }; Button ok = new Button { Content = "OK", IsDefault = true, Width = 70, HorizontalAlignment = HorizontalAlignment.Right }; ok.Click += (s, e) => dialog.DialogResult = true; panel.Children.Add(input); panel.Children.Add(ok); dialog.Content = panel; return dialog.ShowDialog() == true ? input.Text : null; }
        private void WindowPreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Delete && selectedItems.Count > 0) { DeleteSelectedItems(null, null); e.Handled = true; } }
        private void ItemsAreaMouseDown(object sender, MouseButtonEventArgs e) { dragStart = e.GetPosition(itemsDropArea); if (FindTileAt(dragStart) == null) { selectedItems.Clear(); ClearDetails(); UpdateSelectionVisuals(); } }
        private void ItemsAreaMouseMove(object sender, MouseEventArgs e) { if (draggedItem != null || e.LeftButton != MouseButtonState.Pressed) return; Point point = e.GetPosition(itemsDropArea); if ((point - dragStart).Length < 8) return; Button hit = FindTileAt(point); if (hit != null) { LaunchItem item = (LaunchItem)hit.Tag; selectedItems.Add(item); selectedItem = item; UpdateSelectionVisuals(); } }
        private void ItemsAreaMouseUp(object sender, MouseButtonEventArgs e) { }
        private void ItemsAreaMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double next = Math.Max(.6, Math.Min(1.8, Math.Round(cardScale + (e.Delta > 0 ? .1 : -.1), 1)));
            if (Math.Abs(next - cardScale) < .001) { e.Handled = true; return; }
            cardScale = next; Properties.Settings.Default.CardScale = cardScale;
            foreach (Button tile in itemTiles.Values) AnimateCardSize(tile);
            e.Handled = true;
        }
        private void AnimateCardSize(Button tile)
        {
            AnimateSize(tile, FrameworkElement.WidthProperty, 154 * cardScale); AnimateSize(tile, FrameworkElement.HeightProperty, 142 * cardScale);
            var content = tile.Content as StackPanel; var image = content != null && content.Children.Count > 0 ? content.Children[0] as Image : null;
            if (image != null) { AnimateSize(image, FrameworkElement.WidthProperty, 42 * cardScale); AnimateSize(image, FrameworkElement.HeightProperty, 42 * cardScale); }
        }
        private static void AnimateSize(FrameworkElement element, DependencyProperty property, double target)
        {
            double current = property == FrameworkElement.WidthProperty ? element.ActualWidth : element.ActualHeight;
            element.SetValue(property, target);
            var animation = new DoubleAnimation(current, target, TimeSpan.FromMilliseconds(140)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            animation.Completed += (s, e) => element.BeginAnimation(property, null); element.BeginAnimation(property, animation);
        }
        private Button FindTileAt(Point point) { foreach (Button tile in itemTiles.Values) { Point topLeft = tile.TranslatePoint(new Point(0, 0), itemsDropArea); if (new Rect(topLeft, tile.RenderSize).Contains(point)) return tile; } return null; }
        private void UpdateSelectionVisuals() { ShowDetails(); foreach (KeyValuePair<LaunchItem, Button> pair in itemTiles) { bool isActive = selectedItem == pair.Key; bool isSelected = selectedItems.Contains(pair.Key); pair.Value.BorderBrush = isActive || isSelected ? new SolidColorBrush(accentColor) : new SolidColorBrush(Color.FromRgb(80, 80, 80)); pair.Value.BorderThickness = isActive ? new Thickness(4) : (isSelected ? new Thickness(3) : new Thickness(1)); pair.Value.Background = isActive ? new SolidColorBrush(Darken(accentColor, .42)) : (isSelected ? new SolidColorBrush(Darken(accentColor, .3)) : new SolidColorBrush(Color.FromRgb(58, 58, 58))); } }
        private void LaunchSelected(object sender, RoutedEventArgs e) { if (selectedItem != null) TryLaunch(selectedItem); }
        private void LaunchAll(object sender, RoutedEventArgs e) { LaunchWorkspace(workspaceBox.SelectedItem as Workspace); }
        private void LaunchWorkspaceSection(object sender, RoutedEventArgs e) { LaunchWorkspace(((Button)sender).Tag as Workspace); }
        private void LaunchWorkspace(Workspace workspace) { if (workspace == null) return; var failures = launcher.LaunchWorkspace(workspace, (item, process) => sessions.Track(workspace, item, process)); if (failures.Count > 0) MessageBox.Show(string.Join("\n", failures), "起動できなかった項目"); }
        private void TryLaunch(LaunchItem item) { Workspace workspace = FindWorkspace(item); try { Process process = launcher.Launch(item); sessions.Track(workspace, item, process); } catch (Exception ex) { MessageBox.Show(ex.Message, "起動エラー"); } }
        private Workspace FindWorkspace(LaunchItem item) { foreach (Workspace workspace in document.Workspaces) if (workspace.LaunchItems.Contains(item)) return workspace; return workspaceBox.SelectedItem as Workspace; }
        private async void CloseWorkspace(object sender, RoutedEventArgs e) { await CloseWorkspaceAsync(workspaceBox.SelectedItem as Workspace); }
        private async void CloseWorkspaceSection(object sender, RoutedEventArgs e) { await CloseWorkspaceAsync(((Button)sender).Tag as Workspace); }
        private async Task CloseWorkspaceAsync(Workspace workspace)
        {
            if (workspace == null) return;
            int count = sessions.ActiveCount(workspace.Id);
            if (count == 0) { MessageBox.Show("このWorkspaceでLauncherMが追跡している起動中のApplication / Commandはありません。", "Workspaceを閉じる"); return; }
            if (MessageBox.Show(workspace.Name + " から起動した追跡対象 " + count + " 件を閉じますか？", "Workspaceをすべて閉じる", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            var failures = await sessions.CloseAsync(workspace.Id);
            if (failures.Count > 0) MessageBox.Show(string.Join("\n", failures), "閉じられなかった項目");
        }
        private void OpenSelectedFolder(object sender, RoutedEventArgs e) { if (selectedItem == null || string.IsNullOrWhiteSpace(selectedItem.Target)) return; string path = Directory.Exists(selectedItem.Target) ? selectedItem.Target : Path.GetDirectoryName(selectedItem.Target); if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"")); }
        private void OpenSettings(object sender, RoutedEventArgs e) { StructureSettingsEnvironmental settings = new StructureSettingsEnvironmental(); var window = new EnvironmentWindow(ref settings) { Owner = this }; window.AppearanceChanged += ApplyAppearance; window.ShowDialog(); }
        private async void AddItem(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; LaunchItem item = ShowLaunchItemDialog(null); if (item == null) return; workspace.LaunchItems.Add(item); await EnsureWebsiteIcon(item); repository.Save(document); WorkspaceChanged(null, null); }
        private async void AddLink(object sender, RoutedEventArgs e) { Workspace workspace = workspaceBox.SelectedItem as Workspace; if (workspace == null) return; LaunchItem item = ShowLaunchItemDialog(null, LaunchItemType.Url); if (item == null) return; workspace.LaunchItems.Add(item); await EnsureWebsiteIcon(item); repository.Save(document); WorkspaceChanged(null, null); }
        private async void EditItem(object sender, RoutedEventArgs e) { if (selectedItem == null) return; LaunchItem edited = ShowLaunchItemDialog(selectedItem); if (edited == null) return; selectedItem.Name = edited.Name; selectedItem.Type = edited.Type; selectedItem.Target = edited.Target; selectedItem.Arguments = edited.Arguments; selectedItem.WorkingDirectory = edited.WorkingDirectory; selectedItem.Browser = edited.Browser; selectedItem.BrowserProfile = edited.BrowserProfile; selectedItem.BrowserWindowGroup = edited.BrowserWindowGroup; selectedItem.IconPath = edited.IconPath; await EnsureWebsiteIcon(selectedItem); repository.Save(document); RenderItems(); }
        private LaunchItem ShowLaunchItemDialog(LaunchItem source, LaunchItemType? initialType = null)
        {
            Window dialog = new Window { Title = source == null ? "LaunchItemを追加" : "LaunchItemを編集", Width = 540, Height = 760, MaxHeight = SystemParameters.WorkArea.Height, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this, Background = Background, Foreground = Foreground };
            dialog.Resources = Resources;
            StackPanel panel = new StackPanel { Margin = new Thickness(18) };
            TextBox name = AddDialogText(panel, "名前", source?.Name ?? "");
            AddDialogSection(panel, "何を開く？ / Target");
            var type = new ComboBox { ItemsSource = Enum.GetValues(typeof(LaunchItemType)), SelectedItem = source == null ? (object)(initialType ?? LaunchItemType.Application) : source.Type, Margin = new Thickness(0, 2, 0, 8) };
            panel.Children.Add(new TextBlock { Text = "対象の種類" }); panel.Children.Add(type);
            TextBox target = AddDialogText(panel, "対象（URL / パス / コマンド）", source?.Target ?? "");
            AddDialogSection(panel, "何で開く？ / Opener");
            var openerHint = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) }; panel.Children.Add(openerHint);
            var browserPanel = new StackPanel(); panel.Children.Add(browserPanel);
            browserPanel.Children.Add(new TextBlock { Text = "ブラウザ（Default = OS既定）" });
            var browser = new ComboBox { ItemsSource = new[] { "Default", "Chrome", "Edge", "Firefox" }, SelectedItem = string.IsNullOrWhiteSpace(source?.Browser) ? "Default" : source.Browser, Margin = new Thickness(0, 2, 0, 8) }; browserPanel.Children.Add(browser);
            var urlPanel = new StackPanel(); panel.Children.Add(urlPanel);
            urlPanel.Children.Add(new TextBlock { Text = "ブラウザプロファイル（Opener Context）" });
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
            var executionPanel = new StackPanel(); panel.Children.Add(executionPanel);
            TextBox arguments = AddDialogText(executionPanel, "実行時の引数", source?.Arguments ?? "");
            var workingPanel = new StackPanel(); panel.Children.Add(workingPanel);
            TextBox working = AddDialogText(workingPanel, "作業フォルダ（Opener Context）", source?.WorkingDirectory ?? "");
            working.IsReadOnly = true;
            working.ToolTip = "保存済みの実行Context。編集は未対応です。";
            AddDialogSection(panel, "どこに開く？ / Destination");
            var destinationPanel = new StackPanel(); panel.Children.Add(destinationPanel);
            destinationPanel.Children.Add(new TextBlock { Text = "ウィンドウグループ（Destination Context）" });
            var windowGroup = new ComboBox { IsEditable = true, Margin = new Thickness(0, 2, 0, 8) };
            windowGroup.Items.Add(""); windowGroup.Items.Add("Window 1"); windowGroup.Items.Add("Window 2"); windowGroup.Items.Add("Window 3");
            var ownerWorkspace = document.Workspaces.Find(w => source != null && w.LaunchItems.Contains(source)) ?? workspaceBox.SelectedItem as Workspace;
            if (ownerWorkspace != null) foreach (var member in ownerWorkspace.LaunchItems)
                if (!string.IsNullOrWhiteSpace(member.BrowserWindowGroup) && !windowGroup.Items.Contains(member.BrowserWindowGroup)) windowGroup.Items.Add(member.BrowserWindowGroup);
            windowGroup.Text = source?.BrowserWindowGroup ?? ""; destinationPanel.Children.Add(windowGroup);
            var hint = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) }; panel.Children.Add(hint);
            Action updateUrlFields = () => {
                var selectedType = (LaunchItemType)type.SelectedItem;
                bool visible = selectedType == LaunchItemType.Url;
                string selectedBrowser = browser.SelectedItem as string;
                bool grouped = visible && (selectedBrowser == "Chrome" || selectedBrowser == "Edge");
                browserPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                urlPanel.Visibility = visible && selectedBrowser != "Default" ? Visibility.Visible : Visibility.Collapsed;
                destinationPanel.Visibility = grouped ? Visibility.Visible : Visibility.Collapsed;
                executionPanel.Visibility = selectedType == LaunchItemType.Application || selectedType == LaunchItemType.Command ? Visibility.Visible : Visibility.Collapsed;
                workingPanel.Visibility = selectedType == LaunchItemType.Application ? Visibility.Visible : Visibility.Collapsed;
                openerHint.Text = OpenerLabel(selectedType, selectedBrowser);
                hint.Text = grouped ? "一括起動時、同じブラウザ・プロファイル・グループのURLを新しいウィンドウのタブにまとめます。空欄と個別起動はブラウザ任せです。既存ウィンドウの指定ではありません。" : "配置先はOS / アプリに任せます。この種類の配置先指定は未対応です。";
            };
            browser.SelectionChanged += (s, e) => { loadProfiles(""); updateUrlFields(); };
            type.SelectionChanged += (s, e) => updateUrlFields();
            loadProfiles(source?.BrowserProfile); updateUrlFields();
            AddDialogSection(panel, "表示設定");
            panel.Children.Add(new TextBlock { Text = "アイコン（空欄のURLはfaviconを自動取得）" });
            var iconRow = new DockPanel { Margin = new Thickness(0, 2, 0, 10) };
            var iconBrowse = new Button { Content = "参照…", Width = 75, Margin = new Thickness(6, 0, 0, 0) };
            DockPanel.SetDock(iconBrowse, Dock.Right); iconRow.Children.Add(iconBrowse);
            var iconPath = new TextBox { Text = source?.IconPath ?? "", Margin = new Thickness(0) }; iconRow.Children.Add(iconPath); panel.Children.Add(iconRow);
            iconBrowse.Click += (s, e) => { using (var picker = new System.Windows.Forms.OpenFileDialog { Title = "アイコンに使用するファイルを選択", Filter = "アイコン・画像・実行ファイル|*.ico;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.exe;*.lnk|すべてのファイル|*.*" }) if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK) iconPath.Text = picker.FileName; };
            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right }; Button ok = new Button { Content = "OK", Width = 75, IsDefault = true }; Button cancel = new Button { Content = "キャンセル", Width = 90, IsCancel = true, Margin = new Thickness(8, 0, 0, 0) }; buttons.Children.Add(ok); buttons.Children.Add(cancel); panel.Children.Add(buttons); dialog.Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }; ok.Click += (s, e) => { if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(target.Text)) { MessageBox.Show("名前とTargetを入力してください。", "入力確認"); return; } if ((LaunchItemType)type.SelectedItem == LaunchItemType.Url && !Uri.IsWellFormedUriString(target.Text.Trim(), UriKind.Absolute)) { MessageBox.Show("有効なURLを入力してください。", "入力確認"); return; } if (!string.IsNullOrWhiteSpace(iconPath.Text) && !File.Exists(iconPath.Text.Trim())) { MessageBox.Show("指定したアイコンファイルが見つかりません。", "入力確認"); return; } dialog.DialogResult = true; }; if (dialog.ShowDialog() != true) return null; string savedIcon = iconPath.Text.Trim(); if (source != null && websiteIcons.IsManagedPath(savedIcon) && !string.Equals(source.Target, target.Text.Trim(), StringComparison.OrdinalIgnoreCase)) savedIcon = ""; return new LaunchItem { Id = source == null ? Guid.NewGuid() : source.Id, Name = name.Text.Trim(), Type = (LaunchItemType)type.SelectedItem, Target = target.Text.Trim(), Arguments = arguments.Text.Trim(), WorkingDirectory = source == null ? "" : source.WorkingDirectory, Browser = browser.SelectedItem == null ? "Default" : browser.SelectedItem.ToString(), BrowserProfile = profileId(), BrowserWindowGroup = windowGroup.Text.Trim(), IconPath = savedIcon };
        }
        private static void AddDialogSection(StackPanel panel, string title)
        {
            panel.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });
            panel.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(101, 181, 255)), Margin = new Thickness(0, 0, 0, 8) });
        }
        private static string OpenerLabel(LaunchItemType type, string browser)
        {
            switch (type)
            {
                case LaunchItemType.Url: return string.IsNullOrWhiteSpace(browser) || browser == "Default" ? "OS既定のブラウザ" : browser;
                case LaunchItemType.Folder: return "Explorer";
                case LaunchItemType.File: return "OS既定の関連付けアプリ";
                case LaunchItemType.Command: return "cmd.exe /k（終了後もコンソールを維持）";
                default: return "対象のアプリを直接実行";
            }
        }
        private void ShowDetails()
        {
            if (selectedItem == null) { detailFields.Visibility = Visibility.Collapsed; return; }
            var item = selectedItem;
            detailFields.Visibility = Visibility.Visible;
            detailName.Text = item.Name; detailMemo.Text = "作業環境を構成する1つのLaunchItem";
            detailIcon.Source = GetItemIcon(item);
            typeText.Text = item.Type.ToString(); targetText.Text = item.Target;
            openerText.Text = OpenerLabel(item.Type, item.Browser);
            bool url = item.Type == LaunchItemType.Url;
            bool explicitBrowser = url && !string.IsNullOrWhiteSpace(item.Browser) && !string.Equals(item.Browser, "Default", StringComparison.OrdinalIgnoreCase);
            bool grouped = url && (string.Equals(item.Browser, "Chrome", StringComparison.OrdinalIgnoreCase) || string.Equals(item.Browser, "Edge", StringComparison.OrdinalIgnoreCase));
            profileDetails.Visibility = explicitBrowser ? Visibility.Visible : Visibility.Collapsed;
            profileText.Text = string.IsNullOrWhiteSpace(item.BrowserProfile) ? "未指定（ブラウザ任せ）" : item.BrowserProfile;
            argumentDetails.Visibility = item.Type == LaunchItemType.Application || item.Type == LaunchItemType.Command ? Visibility.Visible : Visibility.Collapsed;
            argumentsText.Text = item.Arguments;
            workingDirectoryDetails.Visibility = item.Type == LaunchItemType.Application ? Visibility.Visible : Visibility.Collapsed;
            workingDirectoryText.Text = string.IsNullOrWhiteSpace(item.WorkingDirectory) ? "未指定" : item.WorkingDirectory;
            groupDetails.Visibility = grouped ? Visibility.Visible : Visibility.Collapsed;
            groupText.Text = string.IsNullOrWhiteSpace(item.BrowserWindowGroup) ? "未指定" : item.BrowserWindowGroup;
            destinationText.Text = grouped && !string.IsNullOrWhiteSpace(item.BrowserWindowGroup)
                ? "ブラウザウィンドウ：一括起動時に同じブラウザ・プロファイル・グループを新しいウィンドウのタブにまとめます。個別起動はブラウザ任せです。"
                : "OS / アプリに任せる（配置指定なし）";
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
        private void RenderWorkspaceTabs()
        {
            workspaceTabs.Children.Clear();
            foreach (Workspace workspace in document.Workspaces)
            {
                var tab = new System.Windows.Controls.Primitives.ToggleButton { Content = workspace.Name, Tag = workspace, Style = (Style)FindResource("WorkspaceTabStyle") };
                tab.Click += WorkspaceTabClicked;
                tab.PreviewMouseLeftButtonDown += WorkspaceTabMouseDown;
                tab.PreviewMouseMove += WorkspaceTabMouseMove;
                tab.AllowDrop = true;
                tab.DragOver += WorkspaceTabDragOver;
                tab.Drop += WorkspaceTabDrop;
                workspaceTabs.Children.Add(tab);
            }
            var addTabButton = new Button { Content = "+", Width = 34, Height = 32, FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(0), ToolTip = "Workspaceを追加", Background = new SolidColorBrush(Color.FromRgb(48, 48, 48)), BorderBrush = new SolidColorBrush(Color.FromRgb(96, 96, 96)) };
            addTabButton.Click += AddWorkspace;
            workspaceTabs.Children.Add(addTabButton);
            UpdateWorkspaceTabs();
            ApplyAppearance();
        }
        private void WorkspaceTabClicked(object sender, RoutedEventArgs e)
        {
            Workspace workspace = ((System.Windows.Controls.Primitives.ToggleButton)sender).Tag as Workspace;
            if (workspace != null) workspaceBox.SelectedItem = workspace;
        }
        private void WorkspaceTabMouseDown(object sender, MouseButtonEventArgs e)
        {
            draggedWorkspace = ((System.Windows.Controls.Primitives.ToggleButton)sender).Tag as Workspace;
            workspaceTabDragStart = e.GetPosition(workspaceTabs);
            if (draggedWorkspace != null) workspaceBox.SelectedItem = draggedWorkspace;
        }
        private void WorkspaceTabMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || draggedWorkspace == null) return;
            Point current = e.GetPosition(workspaceTabs);
            if (Math.Abs(current.X - workspaceTabDragStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(current.Y - workspaceTabDragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            Workspace source = draggedWorkspace;
            draggedWorkspace = null;
            BeginDragVisual(workspaceTabs, (FrameworkElement)sender);
            UpdateDragVisual(e.GetPosition(workspaceTabs));
            try { DragDrop.DoDragDrop((DependencyObject)sender, new DataObject(typeof(Workspace), source), DragDropEffects.Move); }
            finally { EndDragVisual(); }
        }
        private void WorkspaceTabDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(Workspace)) ? DragDropEffects.Move : DragDropEffects.None;
            var target = (System.Windows.Controls.Primitives.ToggleButton)sender;
            ShowReorderHint(target, e.GetPosition(target).X >= target.ActualWidth / 2);
            UpdateDragVisual(e.GetPosition(workspaceTabs));
            e.Handled = true;
        }
        private void WorkspaceTabDrop(object sender, DragEventArgs e)
        {
            Workspace source = e.Data.GetData(typeof(Workspace)) as Workspace;
            Workspace target = ((System.Windows.Controls.Primitives.ToggleButton)sender).Tag as Workspace;
            if (source == null || target == null || ReferenceEquals(source, target)) { e.Handled = true; return; }
            var targetTab = (System.Windows.Controls.Primitives.ToggleButton)sender;
            bool insertAfter = e.GetPosition(targetTab).X >= targetTab.ActualWidth / 2;
            MoveWorkspace(source, target, insertAfter);
            e.Handled = true;
        }
        private void WorkspaceTabsDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(Workspace)) ? DragDropEffects.Move : DragDropEffects.None;
            ClearReorderHint();
            UpdateDragVisual(e.GetPosition(workspaceTabs));
            e.Handled = true;
        }
        private void WorkspaceTabsDrop(object sender, DragEventArgs e)
        {
            Workspace source = e.Data.GetData(typeof(Workspace)) as Workspace;
            if (source == null) return;
            document.Workspaces.Remove(source);
            document.Workspaces.Add(source);
            SaveWorkspaceOrder(source);
            e.Handled = true;
        }
        private void MoveWorkspace(Workspace source, Workspace target, bool insertAfter)
        {
            document.Workspaces.Remove(source);
            int targetIndex = document.Workspaces.IndexOf(target);
            document.Workspaces.Insert(targetIndex + (insertAfter ? 1 : 0), source);
            SaveWorkspaceOrder(source);
        }
        private void SaveWorkspaceOrder(Workspace selectedWorkspace)
        {
            repository.Save(document);
            workspaceBox.Items.Refresh();
            workspaceBox.SelectedItem = selectedWorkspace;
            RenderWorkspaceTabs();
            if (showAllWorkspaces) RenderAllWorkspaces();
        }
        private void BeginDragVisual(UIElement host, FrameworkElement source)
        {
            EndDragVisual();
            if (source.ActualWidth <= 0 || source.ActualHeight <= 0) return;
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(source.ActualWidth), (int)Math.Ceiling(source.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(source);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(host);
            if (layer == null) return;
            dragPreviewLayer = layer;
            dragSourceElement = source;
            source.Opacity = 0.38;
            source.Effect = new DropShadowEffect { Color = accentColor, BlurRadius = 14, ShadowDepth = 0, Opacity = 0.8 };
            dragPreview = new DragPreviewAdorner(host, new ImageBrush(bitmap), source.ActualWidth, source.ActualHeight, accentColor);
            layer.Add(dragPreview);
        }
        private void UpdateDragVisual(Point point) { if (dragPreview != null) dragPreview.Update(point); }
        private void EndDragVisual()
        {
            ClearReorderHint();
            if (dragSourceElement != null) { dragSourceElement.Opacity = 1; dragSourceElement.Effect = null; dragSourceElement = null; }
            if (dragPreview == null) return;
            if (dragPreviewLayer != null) dragPreviewLayer.Remove(dragPreview); dragPreview = null; dragPreviewLayer = null;
        }
        private void ShowReorderHint(FrameworkElement target, bool insertAfter)
        {
            if (ReferenceEquals(target, dragSourceElement)) { ClearReorderHint(); return; }
            double offset = insertAfter ? -18 : 18;
            if (ReferenceEquals(reorderHintElement, target) && reorderHintOffset == offset) return;
            ClearReorderHint();
            reorderHintElement = target; reorderHintOffset = offset;
            var transform = new TranslateTransform(); target.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, offset, TimeSpan.FromMilliseconds(130)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } });
        }
        private void ClearReorderHint()
        {
            FrameworkElement target = reorderHintElement; reorderHintElement = null; reorderHintOffset = 0;
            if (target == null) return;
            var transform = target.RenderTransform as TranslateTransform;
            if (transform == null) return;
            var animation = new DoubleAnimation(transform.X, 0, TimeSpan.FromMilliseconds(100)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            animation.Completed += (s, e) => { if (ReferenceEquals(target.RenderTransform, transform)) target.RenderTransform = Transform.Identity; };
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        private void UpdateWorkspaceTabs()
        {
            Workspace current = workspaceBox.SelectedItem as Workspace;
            foreach (object child in workspaceTabs.Children)
            {
                var tab = child as System.Windows.Controls.Primitives.ToggleButton;
                if (tab != null) { bool selected = ReferenceEquals(tab.Tag, current); tab.IsChecked = selected; tab.Background = new SolidColorBrush(selected ? Darken(accentColor, .55) : Color.FromRgb(58, 58, 58)); tab.BorderBrush = new SolidColorBrush(selected ? accentColor : Color.FromRgb(96, 96, 96)); }
            }
        }
        private void ApplyAppearance()
        {
            string fontName = Properties.Settings.Default.UiFontFamily; double scale = Properties.Settings.Default.UiFontScale; string theme = Properties.Settings.Default.UiTheme;
            if (string.IsNullOrWhiteSpace(fontName)) fontName = "Segoe UI"; if (scale < .5 || scale > 2) scale = 1;
            FontFamily = new FontFamily(fontName); ApplyFontScale(this, scale);
            Brush accent = EnvironmentWindow.ThemeAccent(theme); accentColor = ((SolidColorBrush)accent).Color;
            bool light = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase);
            Background = new SolidColorBrush(light ? Color.FromRgb(238, 238, 238) : Color.FromRgb(41, 41, 41)); Foreground = new SolidColorBrush(light ? Colors.Black : Color.FromRgb(242, 242, 242));
            var header = FindAncestor<ScrollViewer>(settingsButton); if (header != null) header.Background = new SolidColorBrush(light ? Color.FromRgb(215, 215, 215) : Color.FromRgb(104, 104, 104));
            var tabBar = FindAncestor<Border>(workspaceTabs); if (tabBar != null) tabBar.Background = new SolidColorBrush(light ? Color.FromRgb(226, 226, 226) : Color.FromRgb(32, 32, 32));
            var leftPane = FindAncestor<Border>(workspaceNameText); if (leftPane != null) leftPane.Background = new SolidColorBrush(light ? Color.FromRgb(245, 245, 245) : Color.FromRgb(48, 48, 48));
            itemsDropArea.Background = new SolidColorBrush(light ? Color.FromRgb(250, 250, 250) : Color.FromRgb(36, 36, 36));
            var details = FindAncestor<ScrollViewer>(detailName); if (details != null) details.Background = new SolidColorBrush(light ? Color.FromRgb(250, 250, 250) : Color.FromRgb(41, 41, 41));
            workspaceNameText.Foreground = accent; workspaceNameEditor.Foreground = accent; ApplyThemeToChildren(this, light, accentColor); UpdateWorkspaceTabs(); UpdateSelectionVisuals();
        }
        private void ApplyFontScale(DependencyObject root, double scale)
        {
            var element = root as FrameworkElement;
            if (element is Control control) { if (!originalFontSizes.ContainsKey(element)) originalFontSizes[element] = control.FontSize; control.FontSize = originalFontSizes[element] * scale; }
            else if (element is TextBlock text) { if (!originalFontSizes.ContainsKey(element)) originalFontSizes[element] = text.FontSize; text.FontSize = originalFontSizes[element] * scale; }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) ApplyFontScale(VisualTreeHelper.GetChild(root, i), scale);
        }
        private void ApplyThemeToChildren(DependencyObject root, bool light, Color accent)
        {
            if (root is TextBox box && !ReferenceEquals(box, workspaceNameEditor)) { box.Background = new SolidColorBrush(light ? Colors.White : Color.FromRgb(41, 41, 41)); box.Foreground = new SolidColorBrush(light ? Colors.Black : Color.FromRgb(242, 242, 242)); }
            if (root is TextBlock text && IsAccent(text.Foreground)) text.Foreground = new SolidColorBrush(accent);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) ApplyThemeToChildren(VisualTreeHelper.GetChild(root, i), light, accent);
        }
        private static bool IsAccent(Brush brush) { var solid = brush as SolidColorBrush; if (solid == null) return false; Color c = solid.Color; return (c.R == 101 && c.G == 181 && c.B == 255) || (c.R == 83 && c.G == 194 && c.B == 139) || (c.R == 176 && c.G == 126 && c.B == 255) || (c.R == 255 && c.G == 157 && c.B == 76); }
        private static Color Darken(Color color, double factor) { return Color.FromRgb((byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor)); }
        private static T FindAncestor<T>(DependencyObject element) where T : DependencyObject { DependencyObject current = element; while (current != null) { T match = current as T; if (match != null) return match; current = VisualTreeHelper.GetParent(current); } return null; }
        private void RestoreSplitterWidths()
        {
            if (Properties.Settings.Default.WorkspacePaneWidth >= workspaceColumn.MinWidth) { workspaceColumn.Width = new GridLength(Properties.Settings.Default.WorkspacePaneWidth); workspacePaneWidthBeforeOverview = Properties.Settings.Default.WorkspacePaneWidth; }
            if (Properties.Settings.Default.DetailsPaneWidth >= detailsColumn.MinWidth) detailsColumn.Width = new GridLength(Properties.Settings.Default.DetailsPaneWidth);
        }
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.WorkspacePaneWidth = showAllWorkspaces ? workspacePaneWidthBeforeOverview : workspaceColumn.ActualWidth;
            Properties.Settings.Default.DetailsPaneWidth = detailsColumn.ActualWidth;
            Properties.Settings.Default.CardScale = cardScale;
            Properties.Settings.Default.Save();
        }
        private void ClearDetails() { selectedItem = null; ShowDetails(); openerText.Text = profileText.Text = groupText.Text = destinationText.Text = ""; detailName.Text = "LaunchItemを選択"; detailMemo.Text = typeText.Text = targetText.Text = argumentsText.Text = workingDirectoryText.Text = ""; detailIcon.Source = null; }

        private sealed class DragPreviewAdorner : Adorner
        {
            private readonly Brush preview;
            private readonly double width;
            private readonly double height;
            private readonly Color accent;
            private Point position;
            public DragPreviewAdorner(UIElement adornedElement, Brush preview, double width, double height, Color accent) : base(adornedElement) { this.preview = preview; this.width = width; this.height = height; this.accent = accent; IsHitTestVisible = false; Opacity = 0.88; }
            public void Update(Point cursor) { position = new Point(cursor.X - width / 2, cursor.Y - height / 2); InvalidateVisual(); }
            protected override void OnRender(DrawingContext drawingContext)
            {
                var rect = new Rect(position, new Size(width, height));
                drawingContext.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(90, 0, 0, 0)), null, new Rect(rect.X + 7, rect.Y + 9, rect.Width, rect.Height), 5, 5);
                drawingContext.DrawRoundedRectangle(preview, new Pen(new SolidColorBrush(accent), 2), rect, 5, 5);
            }
        }
    }
}
