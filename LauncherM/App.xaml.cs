using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LauncherM.Infrastructure;

namespace LauncherM
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (LauncherM.Properties.Settings.Default.LaunchHotkeyEnabled)
            {
                string ignored;
                LauncherShortcutService.Apply(true, LauncherM.Properties.Settings.Default.LaunchHotkeyModifiers, LauncherM.Properties.Settings.Default.LaunchHotkeyKey, out ignored);
            }
        }
    }
}
