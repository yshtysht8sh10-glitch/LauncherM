using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RadioButton = System.Windows.Controls.RadioButton;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LauncherM._01_UI
{

    /// <summary>
    /// EnvironmentWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EnvironmentWindow : Window
    {

        StructureSettingsEnvironmental m_StructureSettingsEnvironmental;    // 環境設定
        public List<RadioButton> m_RdbuttonQuantityLauncher = new List<RadioButton>();  // ランチャー個数ラジオボタンリスト


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EnvironmentWindow(ref StructureSettingsEnvironmental StructureSettingsEnvironmental)
        {
            m_StructureSettingsEnvironmental = StructureSettingsEnvironmental;

            InitializeComponent();
        }


        /// <summary>
        /// Loadedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //*************************************************
            // ランチャー個数ラジオボタンリストにランチャーを登録
            //*************************************************      
            m_RdbuttonQuantityLauncher.Add(rdbuttonQuantityLauncher01);
            m_RdbuttonQuantityLauncher.Add(rdbuttonQuantityLauncher02);
            m_RdbuttonQuantityLauncher.Add(rdbuttonQuantityLauncher03);
            m_RdbuttonQuantityLauncher.Add(rdbuttonQuantityLauncher04);


            //*************************************************
            // 現在の環境設定を画面に反映
            //*************************************************   

        }


        /// <summary>
        /// Closedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {

            //*************************************************
            // 現在の画面を環境設定に反映
            //*************************************************   

        }
    }
}
