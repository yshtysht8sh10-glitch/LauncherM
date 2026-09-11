using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LauncherM
{
    public partial class EnvironmentWindow : Window
    {
        public event Action AppearanceChanged;
        public EnvironmentWindow(ref StructureSettingsEnvironmental settings) { InitializeComponent(); }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var fonts = Fonts.SystemFontFamilies.OrderBy(font => font.Source).ToList(); fontFamilyBox.ItemsSource = fonts;
            fontFamilyBox.SelectedItem = fonts.FirstOrDefault(font => string.Equals(font.Source, Properties.Settings.Default.UiFontFamily, StringComparison.OrdinalIgnoreCase)) ?? fonts.FirstOrDefault(font => font.Source == "Segoe UI");
            var sizes = new List<FontSizeChoice> { new FontSizeChoice("80%", .8), new FontSizeChoice("90%", .9), new FontSizeChoice("100%", 1), new FontSizeChoice("110%", 1.1), new FontSizeChoice("125%", 1.25), new FontSizeChoice("150%", 1.5) };
            fontSizeBox.ItemsSource = sizes; fontSizeBox.SelectedItem = sizes.OrderBy(choice => Math.Abs(choice.Scale - Properties.Settings.Default.UiFontScale)).First();
            themeBox.ItemsSource = new[] { "Dark Blue", "Dark Green", "Dark Purple", "Dark Orange", "Light" }; themeBox.SelectedItem = Properties.Settings.Default.UiTheme; if (themeBox.SelectedItem == null) themeBox.SelectedIndex = 0;
            fontFamilyBox.SelectionChanged += (s, args) => UpdatePreview(); fontSizeBox.SelectionChanged += (s, args) => UpdatePreview(); themeBox.SelectionChanged += (s, args) => UpdatePreview(); UpdatePreview();
        }
        private void UpdatePreview()
        {
            var font = fontFamilyBox.SelectedItem as FontFamily; var size = fontSizeBox.SelectedItem as FontSizeChoice; string theme = themeBox.SelectedItem as string;
            if (font != null) previewText.FontFamily = font; if (size != null) previewText.FontSize = 16 * size.Scale;
            previewText.Foreground = theme == "Light" ? Brushes.Black : ThemeAccent(theme);
        }
        private void ApplyClick(object sender, RoutedEventArgs e) { SaveAppearance(); }
        private void OkClick(object sender, RoutedEventArgs e) { SaveAppearance(); DialogResult = true; }
        private void SaveAppearance()
        {
            var font = fontFamilyBox.SelectedItem as FontFamily; var size = fontSizeBox.SelectedItem as FontSizeChoice;
            if (font == null || size == null || themeBox.SelectedItem == null) return;
            Properties.Settings.Default.UiFontFamily = font.Source; Properties.Settings.Default.UiFontScale = size.Scale; Properties.Settings.Default.UiTheme = themeBox.SelectedItem.ToString(); Properties.Settings.Default.Save();
            AppearanceChanged?.Invoke();
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
