using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LauncherM.Infrastructure;
using LauncherM.Domain;

namespace LauncherM
{
    public partial class EnvironmentWindow : Window
    {
        public event Action AppearanceChanged;
        public event Action ConfigurationImported;
        private readonly WorkspaceDocument workspaceDocument;
        private readonly WorkspaceRepository workspaceRepository;
        public EnvironmentWindow(ref StructureSettingsEnvironmental settings) : this(ref settings, null, null) { }
        public EnvironmentWindow(ref StructureSettingsEnvironmental settings, WorkspaceDocument document, WorkspaceRepository repository) { workspaceDocument = document; workspaceRepository = repository; InitializeComponent(); }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var fonts = Fonts.SystemFontFamilies.OrderBy(font => font.Source).ToList(); fontFamilyBox.ItemsSource = fonts;
            fontFamilyBox.SelectedItem = fonts.FirstOrDefault(font => string.Equals(font.Source, Properties.Settings.Default.UiFontFamily, StringComparison.OrdinalIgnoreCase)) ?? fonts.FirstOrDefault(font => font.Source == "Segoe UI");
            var sizes = new List<FontSizeChoice> { new FontSizeChoice("80%", .8), new FontSizeChoice("90%", .9), new FontSizeChoice("100%", 1), new FontSizeChoice("110%", 1.1), new FontSizeChoice("125%", 1.25), new FontSizeChoice("150%", 1.5) };
            fontSizeBox.ItemsSource = sizes; fontSizeBox.SelectedItem = sizes.OrderBy(choice => Math.Abs(choice.Scale - Properties.Settings.Default.UiFontScale)).First();
            themeBox.ItemsSource = new[] { "Dark Blue", "Dark Green", "Dark Purple", "Dark Orange", "Light" }; themeBox.SelectedItem = Properties.Settings.Default.UiTheme; if (themeBox.SelectedItem == null) themeBox.SelectedIndex = 0;
            hotkeyModifiersBox.ItemsSource = new[] { "CTRL+ALT", "CTRL+SHIFT", "ALT+SHIFT", "CTRL+ALT+SHIFT" };
            hotkeyModifiersBox.SelectedItem = Properties.Settings.Default.LaunchHotkeyModifiers; if (hotkeyModifiersBox.SelectedItem == null) hotkeyModifiersBox.SelectedIndex = 0;
            var keys = Enumerable.Range('A', 26).Select(value => ((char)value).ToString()).Concat(Enumerable.Range(1, 12).Select(value => "F" + value)).ToList();
            hotkeyKeyBox.ItemsSource = keys; hotkeyKeyBox.SelectedItem = Properties.Settings.Default.LaunchHotkeyKey; if (hotkeyKeyBox.SelectedItem == null) hotkeyKeyBox.SelectedItem = "L";
            hotkeyEnabledBox.IsChecked = Properties.Settings.Default.LaunchHotkeyEnabled; UpdateHotkeyFields();
            fontFamilyBox.SelectionChanged += (s, args) => UpdatePreview(); fontSizeBox.SelectionChanged += (s, args) => UpdatePreview(); themeBox.SelectionChanged += (s, args) => UpdatePreview(); UpdatePreview();
        }
        private void UpdatePreview()
        {
            var font = fontFamilyBox.SelectedItem as FontFamily; var size = fontSizeBox.SelectedItem as FontSizeChoice; string theme = themeBox.SelectedItem as string;
            if (font != null) previewText.FontFamily = font; if (size != null) previewText.FontSize = 16 * size.Scale;
            previewText.Foreground = theme == "Light" ? Brushes.Black : ThemeAccent(theme);
        }
        private void HotkeyEnabledChanged(object sender, RoutedEventArgs e) { if (hotkeyFields != null) UpdateHotkeyFields(); }
        private void UpdateHotkeyFields() { hotkeyFields.IsEnabled = hotkeyEnabledBox.IsChecked == true; hotkeyStatusText.Text = hotkeyEnabledBox.IsChecked == true ? "LauncherMを終了している状態でも、このキーで起動できます。" : "起動ショートカットは無効です。"; }
        private void ApplyClick(object sender, RoutedEventArgs e) { SaveSettings(); }
        private void OkClick(object sender, RoutedEventArgs e) { if (SaveSettings()) DialogResult = true; }
        private bool SaveSettings()
        {
            var font = fontFamilyBox.SelectedItem as FontFamily; var size = fontSizeBox.SelectedItem as FontSizeChoice;
            if (font == null || size == null || themeBox.SelectedItem == null || hotkeyModifiersBox.SelectedItem == null || hotkeyKeyBox.SelectedItem == null) return false;
            bool enabled = hotkeyEnabledBox.IsChecked == true; string modifiers = hotkeyModifiersBox.SelectedItem.ToString(); string key = hotkeyKeyBox.SelectedItem.ToString();
            string error; if (!LauncherShortcutService.Apply(enabled, modifiers, key, out error)) { hotkeyStatusText.Text = "保存できませんでした: " + error; MessageBox.Show(error, "起動ショートカットを設定できません"); return false; }
            Properties.Settings.Default.UiFontFamily = font.Source; Properties.Settings.Default.UiFontScale = size.Scale; Properties.Settings.Default.UiTheme = themeBox.SelectedItem.ToString();
            Properties.Settings.Default.LaunchHotkeyEnabled = enabled; Properties.Settings.Default.LaunchHotkeyModifiers = modifiers; Properties.Settings.Default.LaunchHotkeyKey = key; Properties.Settings.Default.Save();
            AppearanceChanged?.Invoke();
            hotkeyStatusText.Text = enabled ? modifiers + "+" + key + " でLauncherMを起動します。" : "起動ショートカットを無効にしました。";
            return true;
        }
        private void ExportConfiguration(object sender, RoutedEventArgs e)
        {
            if (workspaceDocument == null) return;
            var dialog = new Microsoft.Win32.SaveFileDialog { Title = "LauncherM設定をエクスポート", Filter = "LauncherM設定バックアップ (*.json)|*.json", FileName = "LauncherM設定_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json", AddExtension = true, DefaultExt = ".json" };
            if (dialog.ShowDialog(this) != true) return;
            try { LauncherConfigurationBackup.Save(dialog.FileName, LauncherConfigurationBackup.Create(workspaceDocument)); backupStatusText.Text = "エクスポートしました: " + dialog.FileName; }
            catch (Exception ex) { backupStatusText.Text = "エクスポートできませんでした: " + ex.Message; MessageBox.Show(ex.Message, "エクスポートエラー"); }
        }
        private void ImportConfiguration(object sender, RoutedEventArgs e)
        {
            if (workspaceDocument == null || workspaceRepository == null) return;
            var dialog = new Microsoft.Win32.OpenFileDialog { Title = "LauncherM設定をインポート", Filter = "LauncherM設定バックアップ (*.json)|*.json" };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                LauncherConfigurationBackup backup = LauncherConfigurationBackup.Load(dialog.FileName);
                if (MessageBox.Show("現在のWorkspaceと設定を、選択したバックアップの内容で置き換えますか？", "設定をインポート", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                workspaceDocument.Version = backup.WorkspaceDocument.Version;
                workspaceDocument.Workspaces.Clear(); foreach (Workspace workspace in backup.WorkspaceDocument.Workspaces) workspaceDocument.Workspaces.Add(workspace);
                workspaceRepository.Save(workspaceDocument); backup.ApplyPreferences();
                string shortcutError; LauncherShortcutService.Apply(backup.Preferences.LaunchHotkeyEnabled, backup.Preferences.LaunchHotkeyModifiers, backup.Preferences.LaunchHotkeyKey, out shortcutError);
                ConfigurationImported?.Invoke(); AppearanceChanged?.Invoke();
                backupStatusText.Text = "インポートしました: " + dialog.FileName;
                if (!string.IsNullOrWhiteSpace(shortcutError)) MessageBox.Show(shortcutError, "起動ショートカットだけ反映できませんでした");
                DialogResult = true;
            }
            catch (Exception ex) { backupStatusText.Text = "インポートできませんでした: " + ex.Message; MessageBox.Show(ex.Message, "インポートエラー"); }
        }
        internal static Brush ThemeAccent(string theme)
        {
            if (theme == "Dark Green") return new SolidColorBrush(Color.FromRgb(83, 194, 139));
            if (theme == "Dark Purple") return new SolidColorBrush(Color.FromRgb(176, 126, 255));
            if (theme == "Dark Orange") return new SolidColorBrush(Color.FromRgb(255, 157, 76));
            return new SolidColorBrush(Color.FromRgb(101, 181, 255));
        }
        private sealed class FontSizeChoice { public string Label { get; private set; } public double Scale { get; private set; } public FontSizeChoice(string label, double scale) { Label = label; Scale = scale; } }
    }
}
