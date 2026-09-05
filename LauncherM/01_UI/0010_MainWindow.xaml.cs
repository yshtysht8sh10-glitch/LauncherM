using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;


namespace LauncherM
{
    /// <summary>
    /// メインフォーム
    /// </summary>
    public partial class MainWindow : Window
    {
        
        /// <summary>
        /// クラス内で自由に参照できる変数
        /// </summary>
        GeneralPurpose m_GeneralPurpose = new GeneralPurpose();             // 汎用クラスのインスタンス
        CharacterString m_CharacterString = new CharacterString();          // 言語別文字列クラスのインスタンス

        StructureDataSave m_DataSaveNoChange;                               // 保存データ<読込時のまま>
        StructureDataSave m_DataSaveEditing;                                // 保存データ<編集中>

        public List<StackPanel> m_StackLauncher = new List<StackPanel>();   // ランチャーのリスト
                                                                            // ※Tabにランチャー番号を格納している

        public List<List<Button>> m_ListListButtonElementLink = new List<List<Button>>();       // リンク要素ボタンリストのリスト
                                                                                                // ※Tabに表示順序を格納している
                                                                                                // ※[ランチャー番号][表示順序]

        ItemLangage m_SettingLangage = ItemLangage.Japanese;                // 指定言語

        int m_SelectedNoAffiliationLuncher;                                 // 選択中リンク要素の所属するランチャー番号                           
        int m_SelectedOrderDisplayInLuncher;                                // 選択中リンク要素の表示順序      

        ColorSet m_ColorSet = ColorSet.Dark01;                                // 指定カラーセット    

        StructureSettingsEnvironmental m_StructureSettingsEnvironmental = new StructureSettingsEnvironmental();    // 環境設定

        Point m_PositionMouseWhenMouseDown;

        /// <summary>
        /// 構造体的に使用するクラス__保存データ
        /// </summary>
        /// <remarks> ※状態保存ファイルに読み書きするデータはこのクラスに格納する </remarks>
        public class StructureDataSave
        {            
            // データ部
            public List<StructureDataSavePartData> PartData;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public StructureDataSave()
            {
                PartData = new List<StructureDataSavePartData>();
            }

            /// <summary>
            /// 複製
            /// </summary>
            public StructureDataSave Copy()
            {
                StructureDataSave dataSave = new StructureDataSave();
                dataSave.PartData = new List<StructureDataSavePartData>(PartData);

                return dataSave;
            }

        }


        /// <summary>
        /// 構造体的に使用するクラス__状態保存データのデータ部
        /// </summary>
        public class StructureDataSavePartData
        {
            public int NoAffiliationLuncher;                    // 所属するランチャーの番号                           
            public int OrderDisplayInLuncher;                   // ランチャー内の表示順序                           
            public string PathExe;                              // exeファイルパス                           
            public string PathImageIcon;                        // アイコン画像パス                           
            public string PathImagevisual;                      // ビジュアル画像パス                         
            public string StringTitle;                          // タイトル文字列                        
            public string StringMemo;                           // メモ文字列                                
            public List<string> PathFileSelect;                 // select.def(.lua)パス

            public StructureDataSavePartData()
            {
                NoAffiliationLuncher = -1;                      // 所属するランチャーの番号                           
                OrderDisplayInLuncher = -1;                     // ランチャー内の表示順序                           
                PathExe = string.Empty;                         // exeファイルパス                           
                PathImageIcon = string.Empty;                   // アイコン画像パス                           
                PathImagevisual = string.Empty;                 // ビジュアル画像パス                         
                StringTitle = string.Empty;                     // タイトル文字列                        
                StringMemo = string.Empty;                      // メモ文字列                                
                PathFileSelect = new List<string>();            // select.def(.lua)パス
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// メイン画面のLoadedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //*************************************************
            // Formのプロパティ設定
            //*************************************************    
            this.SizeToContent = SizeToContent.Width;

            //*************************************************
            // 下地のプロパティ設定
            //*************************************************    
            //コンテキストメニュー
            ContextMenu contextMenuForm = new ContextMenu();
            MenuItem menumDispMenu = new MenuItem();
            menumDispMenu.Header = m_CharacterString.DispMenu[(int)m_SettingLangage];
            menumDispMenu.Click += contextMenuDispMenu_Click;
            contextMenuForm.Items.Add(menumDispMenu);
            dockBody.ContextMenu = contextMenuForm;

            //*************************************************
            // ランチャーのプロパティ設定
            //*************************************************     
            // 幅       
            stackLauncher00.Width = Const.WidthStackLauncher;     // ランチャー00
            stackLauncher01.Width = Const.WidthStackLauncher;     // ランチャー01
            stackLauncher02.Width = Const.WidthStackLauncher;     // ランチャー02
            stackLauncher03.Width = Const.WidthStackLauncher;     // ランチャー03

            //コンテキストメニュー
            ContextMenu contextMenuLauncher = new ContextMenu();
            MenuItem menumDispMenuSecond = new MenuItem();
            menumDispMenuSecond.Header = m_CharacterString.DispMenu[(int)m_SettingLangage];
            menumDispMenuSecond.Click += contextMenuDispMenu_Click;
            contextMenuLauncher.Items.Add(menumDispMenuSecond);
            MenuItem menuItemAdd = new MenuItem();
            menuItemAdd.Header = m_CharacterString.Addition[(int)m_SettingLangage];
            menuItemAdd.Click += contextMenuItemAdd_Click;
            contextMenuLauncher.Items.Add(menuItemAdd);
            stackLauncher00.ContextMenu = contextMenuLauncher;
            stackLauncher01.ContextMenu = contextMenuLauncher;
            stackLauncher02.ContextMenu = contextMenuLauncher;
            stackLauncher03.ContextMenu = contextMenuLauncher;

            //*************************************************
            // ランチャーリストに，ランチャーを登録
            // ※リストの要素番号とstackLauncherのTagの数字が一致している
            //*************************************************            
            m_StackLauncher.Add(stackLauncher00);     // ランチャー00
            m_StackLauncher.Add(stackLauncher01);     // ランチャー01
            m_StackLauncher.Add(stackLauncher02);     // ランチャー02
            m_StackLauncher.Add(stackLauncher03);     // ランチャー03

            for (int i = 0; i < Const.MaxQuantityStackLauncher; i++)
            {
                m_ListListButtonElementLink.Add(new List<Button>());
            }

            //*************************************************
            // 初期から1個だけあるリンク要素を削除
            //*************************************************
            buttonElementLinkInit.Visibility = Visibility.Collapsed;

            //*************************************************
            // 状態保存ファイルから状態を復元
            //*************************************************
            // 状態保存ファイル読み込み
            m_DataSaveNoChange = ReadDataEdited();
            m_DataSaveEditing = m_DataSaveNoChange.Copy();

            // ランチャー毎に処理
            for (int numLauncher = 0; numLauncher < Const.MaxQuantityStackLauncher; numLauncher++)                     // ランチャーの数分ループ
            {

                // コントロールを先に全て作成する
                int inPartData = CountElementLinkInPartData(m_DataSaveEditing, numLauncher);
                for (int numElementLink = 0; numElementLink < inPartData; numElementLink++)
                {
                    AddElementLinkNew(m_StackLauncher[numLauncher]);
                }

                // リンク要素コントロールに状態を反映
                for (int numElementLink = 0; numElementLink < CountElementLinkInPartData(m_DataSaveEditing, numLauncher); numElementLink++)
                {

                    // リンク要素の状態を表示順序の若い順に取得
                    StructureDataSavePartData dataSavePartData = GetDataSaveThisElementLink(numLauncher, numElementLink, m_DataSaveEditing);

                    // リンク要素内のイメージに状態を反映
                    if(dataSavePartData.PathImageIcon != null)
                    {
                        if (!dataSavePartData.PathImageIcon.Equals(string.Empty))
                        {
                            if (System.IO.File.Exists(dataSavePartData.PathImageIcon))
                            {// イメージファイルの存在を確認
                                Image image = (Image)((StackPanel)m_ListListButtonElementLink[numLauncher][numElementLink].Content).Children[0];
                                image.Source = new BitmapImage(new Uri(dataSavePartData.PathImageIcon));
                            }
                        }
                    }

                    // リンク要素内のテキストブロックに状態を反映
                    if (dataSavePartData.StringTitle != null)
                    {
                        if (!dataSavePartData.StringTitle.Equals(string.Empty))
                        {
                            TextBlock textblock = (TextBlock)((StackPanel)m_ListListButtonElementLink[numLauncher][numElementLink].Content).Children[1];
                            textblock.Text = dataSavePartData.StringTitle;
                        }
                    }

                }

            }

            //*************************************************
            // 初期選択
            //*************************************************
            if (m_ListListButtonElementLink[0].Count > 0)
            {
                buttonElementLink_Click(m_ListListButtonElementLink[0][0], null);    // ランチャー00のリンク要素00を選択状態とする
            }

            //*************************************************
            // レイアウト設定
            //*************************************************
            SettingUI(m_ColorSet);

            //*************************************************
            // 環境設定ファイルから環境を復元
            //*************************************************
            m_StructureSettingsEnvironmental = ReadSettingsEnvironmental();

            // 各種領域の表示設定
            if (m_StructureSettingsEnvironmental.DispMenu)
            {
                stackMenu.Visibility = Visibility.Visible;
            }
            else
            {
                stackMenu.Visibility = Visibility.Collapsed;
            }

            if (m_StructureSettingsEnvironmental.DispLauncher00)
            {
                stackLauncher00.Visibility = Visibility.Visible;
            }
            else
            {
                stackLauncher00.Visibility = Visibility.Collapsed;
            }

            if (m_StructureSettingsEnvironmental.DispLauncher01)
            {
                stackLauncher01.Visibility = Visibility.Visible;
            }
            else
            {
                stackLauncher01.Visibility = Visibility.Collapsed;
            }

            if (m_StructureSettingsEnvironmental.DispLauncher02)
            {
                stackLauncher02.Visibility = Visibility.Visible;
            }
            else
            {
                stackLauncher02.Visibility = Visibility.Collapsed;
            }

            if (m_StructureSettingsEnvironmental.DispLauncher03)
            {
                stackLauncher03.Visibility = Visibility.Visible;
            }
            else
            {
                stackLauncher03.Visibility = Visibility.Collapsed;
            }

            if (m_StructureSettingsEnvironmental.DispEditElementLink)
            {
                stackEditElementLink.Visibility = Visibility.Visible;
                stackPartitionVertical01.Visibility = Visibility.Visible;
            }
            else
            {
                stackEditElementLink.Visibility = Visibility.Collapsed;
                stackPartitionVertical01.Visibility = Visibility.Collapsed;
            }

            if (m_StructureSettingsEnvironmental.DispEditSpecial)
            {
                stackEditSpecial.Visibility = Visibility.Visible;
                stackPartitionVertical02.Visibility = Visibility.Visible;
            }
            else
            {
                stackEditSpecial.Visibility = Visibility.Collapsed;
                stackPartitionVertical02.Visibility = Visibility.Collapsed;

                if (stackEditElementLink.Visibility == Visibility.Visible)
                {
                    dockBody.Children.Remove(stackEditElementLink);         // リンク要素編集部をストレッチ幅にすべく，記述準を末尾に移動
                    dockBody.Children.Insert(4, stackEditElementLink);      // 〃
                    stackEditElementLink.Width = double.NaN;
                }
            }

        }


        /// <summary>
        /// メイン画面のClosingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WriteDataEdited(m_DataSaveEditing);                             // 編集データのファイル保存
            WriteSettingsEnvironmental(m_StructureSettingsEnvironmental);   // 環境設定のファイル保存
        }


        /// <summary>
        /// メインイメージボタン クリックのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonMainImage_Click(object sender, RoutedEventArgs e)
        {
            
            //*************************************************
            // ファイルダイアログを開く
            //*************************************************
            //ダイアログを生成
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();

            //ダイアログのタイトルを指定する
            openDialog.Title = m_CharacterString.SelectAnImage[(int)m_SettingLangage];

            //ダイアログを表示する
            StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);
            if (openDialog.ShowDialog() == true)
            {
                // 選んだ画像を編集データとして保存
                dataSave.PathImageIcon = openDialog.FileName;
                dataSave.PathImagevisual = openDialog.FileName;

                // 選んだ画像をメインイメージに反映
                Button button = (Button)sender;
                Image imageButtonMainImage = (Image)button.Content;
                imageButtonMainImage.Source = new BitmapImage(new Uri(dataSave.PathImagevisual));

                // 選んだ画像をリンク要素に反映
                Image imageButtonDataLink = (Image)((StackPanel)m_ListListButtonElementLink[m_SelectedNoAffiliationLuncher][m_SelectedOrderDisplayInLuncher].Content).Children[0];
                imageButtonDataLink.Source = imageButtonMainImage.Source;
            }
                        
        }
        

        /// <summary>
        /// ランチャーのマウスホイールスクロールイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void stackLauncher_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            StackPanel stack = (StackPanel)sender;

            double top = stack.Margin.Top + e.Delta;
            if (top > 0)
            {
                top = 0;
            }
            stack.Margin = new Thickness(0, top, 0, 0);
        }


        /// <summary>
        /// リンク要素ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void buttonElementLink_Click(object sender, RoutedEventArgs e)
        {

            //*************************************************
            // イベントを起したボタンの情報を抽出
            //*************************************************
            Button button = (Button)sender;                                                 // 当コントロール
            Image image = (Image)((StackPanel)button.Content).Children[0];                  // 子コントロール
            TextBlock textblock = (TextBlock)((StackPanel)button.Content).Children[1];      // 子コントロール
            StackPanel stack = (StackPanel)((ContentControl)button.Parent).Parent;          // 親コントロール

            //*************************************************
            // 選択リンク要素を記録
            //*************************************************
            m_SelectedNoAffiliationLuncher = int.Parse(stack.Tag.ToString());
            m_SelectedOrderDisplayInLuncher = int.Parse(button.Tag.ToString());

            //*************************************************
            // 選択リンク要素の関連データを取得
            //*************************************************
            StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);

            //*************************************************
            // メインイメージボタンを設定
            //*************************************************
            Image imageButtonMainImage = (Image)buttonMainImage.Content;
            if (dataSave.PathImageIcon != string.Empty)
            {
                if (System.IO.File.Exists(dataSave.PathImageIcon))
                {// イメージファイルの存在を確認
                    imageButtonMainImage.Source = new BitmapImage(new Uri(dataSave.PathImageIcon));
                }
            }

            //*************************************************
            // アイコンを設定
            //*************************************************
            Image imageButtonLink = (Image)buttonLink.Content;
            imageButtonLink.Source = m_GeneralPurpose.GetIconFromFilePathSpecified(dataSave.PathExe);

            //*************************************************
            // パス表示ラベルを設定
            //*************************************************
            textblockFullPath.Text = dataSave.PathExe;

            //*************************************************
            // タイトルラベルを設定
            //*************************************************
            if (textblock.Text == null)
            {
                textboxTitle.Text = string.Empty;
            }
            else
            {
                textboxTitle.Text = textblock.Text;
            }

            //*************************************************
            // メモラベルを設定
            //*************************************************
            textMemo.Text = dataSave.StringMemo;

        }


        /// <summary>
        /// ランチャーのコンティストメニューの"追加"クリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuItemAdd_Click(object sender, RoutedEventArgs e)
        {

            //*************************************************
            // リンク要素を追加
            //*************************************************
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            StackPanel stackPanel = (StackPanel)contextMenu.PlacementTarget;
            AddElementLinkNew(m_StackLauncher[int.Parse(stackPanel.Tag.ToString())]);

            //*************************************************
            // カラー設定
            //*************************************************
            SettingColor(m_ColorSet);

        }


        /// <summary>
        /// Formのコンティストメニューの"メニュー表示"クリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuDispMenu_Click(object sender, RoutedEventArgs e)
        {
            buttonDispMenu_Click(sender, e);
        }

        /// <summary>
        /// リンク要素のコンティストメニューの"削除"クリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {

            //*************************************************
            // リンク要素を削除
            //*************************************************
            MenuItem menuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            Button button = (Button)contextMenu.PlacementTarget;
            DeleteElementLinkNew(button);

        }


        /// <summary>
        /// 環境設定ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSettingEnvironment_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentWindow Window = new EnvironmentWindow(ref m_StructureSettingsEnvironmental);
            Window.Owner = this;
            Window.ShowDialog();

            // ランチャーの表示個数を設定

            // レイアウト設定
            SettingUI(m_ColorSet);
        }

        /// <summary>
        /// リンク要素タイトル編集テキストボックスのTextChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textboxTitle_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (m_DataSaveEditing != null)
            {

                // 反映元を抽出
                TextBox textbox = (TextBox)sender;

                // タイトルの編集内容を編集データに保存
                StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);
                dataSave.StringTitle = textbox.Text;

                // タイトルの編集内容をリンク要素に反映
                TextBlock textblockButtonDataLink = (TextBlock)((StackPanel)m_ListListButtonElementLink[m_SelectedNoAffiliationLuncher][m_SelectedOrderDisplayInLuncher].Content).Children[1];
                textblockButtonDataLink.Text = textbox.Text;
            }

        }

        /// <summary>
        /// リンク要素メモ編集テキストボックスのTextChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textMemo_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (m_DataSaveEditing != null)
            {

                // 反映元を抽出
                TextBox textbox = (TextBox)sender;

                // 編集内容を編集データに保存
                StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);
                dataSave.StringMemo = textbox.Text;
            }

        }
        
        /// <summary>
        /// リンクボタンのClickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLink_Click(object sender, RoutedEventArgs e)
        {

            //*************************************************
            // ファイルダイアログを開く
            //*************************************************
            //ダイアログを生成
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();

            //ダイアログのタイトルを指定する
            openDialog.Title = m_CharacterString.SelectLinkFile[(int)m_SettingLangage];

            //ダイアログを表示する
            StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);
            if (openDialog.ShowDialog() == true)
            {
                // 選んだファイルのPathを編集データとして保存
                dataSave.PathExe = openDialog.FileName;

                // 選んだファイルのPathをラベルに表示
                textblockFullPath.Text = dataSave.PathExe;

                // 選んだファイルのアイコンをボタンに表示
                Image image = (Image)buttonLink.Content;
                image.Source = m_GeneralPurpose.GetIconFromFilePathSpecified(dataSave.PathExe);
            }
            
        }


        /// <summary>
        /// 実行ボタンClickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonStarting_Click(object sender, RoutedEventArgs e)
        {
            StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);
            if (!dataSave.PathExe.Equals(string.Empty))
            {
                //// コンソール・ウィンドウを開かずにコンソール・アプリケーションを実行
                //ProcessStartInfo psInfo = new ProcessStartInfo();
                //psInfo.FileName = dataSave.PathExe;     // 実行するファイル
                //psInfo.CreateNoWindow = true;           // コンソール・ウィンドウを開かない
                //psInfo.UseShellExecute = false;         // シェル機能を使用しない
                //Process.Start(psInfo);

                //// 普通に起動
                //Process.Start(dataSave.PathExe);

                //// コマンドライン経由で起動
                //Process cmd = new Process();
                //cmd.StartInfo.UseShellExecute = false;
                //cmd.StartInfo.FileName = dataSave.PathExe;
                //cmd.Start();

                // カレントディレクトリを呼び出し先のファイルの場所で起動
                Process p = new Process();
                p.StartInfo.FileName = dataSave.PathExe;
                p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(dataSave.PathExe);
                p.Start();
            }
        }


        /// <summary>
        /// ファイルの場所を開くボタンClickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            StructureDataSavePartData dataSave = GetDataSaveThisElementLink(m_SelectedNoAffiliationLuncher, m_SelectedOrderDisplayInLuncher, m_DataSaveEditing);

            if (!dataSave.PathExe.Equals(string.Empty))
            {
                Process.Start("EXPLORER.EXE", @"/select," + dataSave.PathExe + "");
            }
        }

        /// <summary>
        /// 表示非表示切替ボタン [メニュー] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispMenu_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackMenu.Visibility == Visibility.Visible)
            {
                // 対象を非表示にする 
                stackMenu.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispMenu = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispMenu.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispMenu.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }
            else
            {
                // 対象を表示する 
                stackMenu.Visibility = Visibility.Visible;

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispMenu = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispMenu.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispMenu.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
        }

        /// <summary>
        /// 表示非表示切替ボタン [ランチャー00] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispLauncher00_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackLauncher00.Visibility == Visibility.Visible)
            {
                // 対象を非表示にする 
                stackLauncher00.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispLauncher00 = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispLauncher00.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher00.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }
            else
            {
                // 対象を表示する 
                stackLauncher00.Visibility = Visibility.Visible;

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispLauncher00 = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispLauncher00.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher00.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }

            // ボーダー1の表示/非表示
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical01.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical01.Visibility = Visibility.Visible;
                }
            }

            // ボーダー2の表示/非表示
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible
                    && stackEditElementLink.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical02.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical02.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 表示非表示切替ボタン [ランチャー01] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispLauncher01_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackLauncher01.Visibility == Visibility.Visible)
            {
                // 対象を非表示にする 
                stackLauncher01.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispLauncher01 = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispLauncher01.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher01.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }
            else
            {
                // 対象を表示する 
                stackLauncher01.Visibility = Visibility.Visible;

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispLauncher01 = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispLauncher01.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher01.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }

            // ボーダー1の表示/非表示
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical01.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical01.Visibility = Visibility.Visible;
                }
            }

            // ボーダー2の表示/非表示
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible
                    && stackEditElementLink.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical02.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical02.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 表示非表示切替ボタン [ランチャー02] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispLauncher02_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackLauncher02.Visibility == Visibility.Visible)
            {
                // 対象を非表示にする 
                stackLauncher02.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispLauncher02 = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispLauncher02.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher02.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }
            else
            {
                // 対象を表示する 
                stackLauncher02.Visibility = Visibility.Visible;

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispLauncher02 = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispLauncher02.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher02.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }

            // ボーダー1の表示/非表示
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical01.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical01.Visibility = Visibility.Visible;
                }
            }

            // ボーダー2の表示/非表示
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible
                    && stackEditElementLink.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical02.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical02.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 表示非表示切替ボタン [ランチャー03] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispLauncher03_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackLauncher03.Visibility == Visibility.Visible)
            {
                // 対象を非表示にする 
                stackLauncher03.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispLauncher03 = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispLauncher03.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher03.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }
            else
            {
                // 対象を表示する 
                stackLauncher03.Visibility = Visibility.Visible;

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispLauncher03 = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispLauncher03.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher03.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }

            // ボーダー1の表示/非表示
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical01.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical01.Visibility = Visibility.Visible;
                }
            }

            // ボーダー2の表示/非表示
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible
                    && stackEditElementLink.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical02.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical02.Visibility = Visibility.Visible;
                }
            }

        }

        /// <summary>
        /// 表示非表示切替ボタン [リンク要素編集部] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispEditElementLink_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {// リンク要素編集部を非表示にする流れ


                // 対象を非表示にする 
                stackEditElementLink.Visibility = Visibility.Collapsed;
                stackPartitionVertical01.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispEditElementLink = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispEditElementLink.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispEditElementLink.BorderBrush = color.ColorBorderOffButtonVisible_Menu;

            }
            else
            {// リンク要素編集部を表示する流れ

                // 対象を表示する 
                stackEditElementLink.Visibility = Visibility.Visible;
                stackPartitionVertical01.Visibility = Visibility.Visible;
                if (stackEditSpecial.Visibility == Visibility.Visible)
                {// 特別編集部が表示

                    // リンク要素編集部の横幅設定
                    stackEditElementLink.Width = Const.WidthStackEditElementLink;    
                }
                else
                {// 特別編集部が非表示
                    dockBody.Children.Remove(stackEditElementLink);         // リンク要素編集部をストレッチ幅にすべく，記述準を末尾に移動
                    dockBody.Children.Insert(4, stackEditElementLink);      // 〃
                    stackEditElementLink.Width = double.NaN;
                }

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispEditElementLink = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispEditElementLink.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispEditElementLink.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }

            // ボーダー1の表示/非表示
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical01.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical01.Visibility = Visibility.Visible;
                }
            }

            // ボーダー2の表示/非表示
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible
                    && stackEditElementLink.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical02.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical02.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 表示非表示切替ボタン [特別編集部] Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDispEditSpecial_Click(object sender, RoutedEventArgs e)
        {
            // Form幅の自動調整 ※カーソルで拡縮していると解ける様に思われる
            this.SizeToContent = SizeToContent.Width;

            // 色を取得
            ColorToolThis color = new ColorToolThis(m_ColorSet);

            // 現在の表示状態毎に処理を分岐
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {// 特別編集部を非表示にする流れ

                // 対象を非表示にする 
                stackEditSpecial.Visibility = Visibility.Collapsed;     // 特別編集部を非表示にする
                dockBody.Children.Remove(stackEditElementLink);         // リンク要素編集部をストレッチ幅にすべく，記述準を末尾に移動
                dockBody.Children.Insert(4, stackEditElementLink);      // 〃
                stackEditElementLink.Width = double.NaN;
                stackPartitionVertical02.Visibility = Visibility.Collapsed;

                // 環境設定に対象コントロールが非表示であることを保存 
                m_StructureSettingsEnvironmental.DispEditSpecial = false;

                // 表示/非表示切替ボタンを非表示色へ設定 
                buttonDispEditSpecial.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispEditSpecial.BorderBrush = color.ColorBorderOffButtonVisible_Menu;

            }
            else
            {// 特別編集部を表示する流れ

                // 対象を表示する 
                stackEditSpecial.Visibility = Visibility.Visible;       // 特別編集部を表示する
                dockBody.Children.Remove(stackEditSpecial);             // 特別編集部をストレッチ幅にすべく，記述準を末尾に移動
                dockBody.Children.Insert(4, stackEditSpecial);          // 〃
                stackPartitionVertical02.Visibility = Visibility.Visible;

                // 環境設定に対象コントロールが表示されていることを保存 
                m_StructureSettingsEnvironmental.DispEditSpecial = true;

                // 表示/非表示切替ボタンを表示色へ設定 
                buttonDispEditSpecial.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispEditSpecial.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }

            // ボーダー2の表示/非表示
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                if (stackLauncher00.Visibility != Visibility.Visible
                    && stackLauncher01.Visibility != Visibility.Visible
                    && stackLauncher02.Visibility != Visibility.Visible
                    && stackLauncher03.Visibility != Visibility.Visible
                    && stackEditElementLink.Visibility != Visibility.Visible)
                {
                    stackPartitionVertical02.Visibility = Visibility.Collapsed;
                }
                else
                {
                    stackPartitionVertical02.Visibility = Visibility.Visible;
                }
            }
        }







        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(@"F:\作業用\06_便利ツール\ConfirmDspMsgBox.exe");
        }

        private void buttonElementLinkInit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (Mouse.LeftButton == MouseButtonState.Pressed)
            //{
            //    Button button = (Button)sender;
            //    int MousPointClientXNow = System.Windows.Forms.Cursor.Position;  // 現在のマウスポインタのX座標(this基準)
            //    int MousPointClientYNow = this.PointToClient(System.Windows.Forms.Cursor.Position).Y;  // 現在のマウスポインタのX座標(this基準)
            //    m_PositionMouseWhenMouseDown = new Point();
            //}
        }

        private void buttonElementLinkInit_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void buttonElementLinkInit_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }


        private bool _inDrag = false;
        private double _diffX;
        private double _diffY;
        private void ellipse1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //_inDrag = true;
            //Point point = e.GetPosition(canvas1);
            //_diffX = point.X - Canvas.GetLeft(ellipse1);
            //_diffY = point.Y - Canvas.GetTop(ellipse1);
            //ellipse1.CaptureMouse();
        }
        private void ellipse1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (_inDrag)
            //{
            //    Point pos = e.GetPosition(canvas1);
            //    Canvas.SetLeft(ellipse1, pos.X - _diffX);
            //    Canvas.SetTop(ellipse1, pos.Y - _diffY);
            //}
        }
        private void ellipse1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //_inDrag = false;
            //ellipse1.ReleaseMouseCapture();
        }
    }
}
