using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace LauncherM
{

    /// <summary>
    /// 専用クラス(本ツールと結びついている機能をまとめたクラス　※ メインフォームと同じクラス．ファイルを分けただけ．)
    /// </summary>
    partial class MainWindow : Window
    {


        /// <summary>
        /// レイアウト設定
        /// </summary>
        private void SettingUI(ColorSet ColorSet)
        {

            //*************************************************
            // 定数
            //*************************************************
            const int WIDTH_BORDER = 2;
            const int WIDTH_IMAGE_IN_ELEMENT_LINK = 35;
            const int HEIGHT_IMAGE_IN_ELEMENT_LINK = 35;
            const int WIDTH_TEXTBLOCK_IN_ELEMENT_LINK = 60;
            const int HEIGHT_TEXTBLOCK_IN_ELEMENT_LINK = 35;
            const int HEIGHT_ELEMENT_LINK = 70;


            //*************************************************
            // メニュー部
            //*************************************************

            // サイズ設定
            stackMenu.Height = 40;
            buttonSettingEnvironment.Height = stackMenu.Height;
            buttonSettingEnvironment.Width = buttonSettingEnvironment.Height;

            //*************************************************
            // 横仕切
            //*************************************************
            stackPartitionHorizontal.Height = WIDTH_BORDER;

            //*************************************************
            // 縦仕切01
            //*************************************************
            stackPartitionVertical01.Width = WIDTH_BORDER;

            //*************************************************
            // ランチャー部
            //*************************************************
            // リンク要素
            for (int numLauncher = 0; numLauncher < Const.MaxQuantityStackLauncher; numLauncher++)
            {  // ランチャーの数分ループ


                for (int numElementLink = 0; numElementLink < CountElementLinkInPartData(m_DataSaveEditing, numLauncher); numElementLink++)
                {// リンク要素の数分ループ

                    // リンク要素の状態を表示順序の若い順に取得
                    StructureDataSavePartData dataSavePartData = GetDataSaveThisElementLink(numLauncher, numElementLink, m_DataSaveEditing);

                    // リンク要素内のイメージのサイズを決定
                    Image image = (Image)((StackPanel)m_ListListButtonElementLink[numLauncher][numElementLink].Content).Children[0];
                    image.Width = WIDTH_IMAGE_IN_ELEMENT_LINK;
                    image.Height = HEIGHT_IMAGE_IN_ELEMENT_LINK;

                    // リンク要素内のテキストブロックのサイズを決定
                    TextBlock textblock = (TextBlock)((StackPanel)m_ListListButtonElementLink[numLauncher][numElementLink].Content).Children[1];
                    textblock.Width = WIDTH_TEXTBLOCK_IN_ELEMENT_LINK;
                    textblock.Height = HEIGHT_TEXTBLOCK_IN_ELEMENT_LINK;

                    // リンク要素のサイズを決定
                    m_ListListButtonElementLink[numLauncher][numElementLink].Height = HEIGHT_ELEMENT_LINK;
                }

            }

            //*************************************************
            // 縦仕切02
            //*************************************************
            stackPartitionVertical02.Width = WIDTH_BORDER;

            //*************************************************
            // リンク要素編集部
            //*************************************************
            // リンク要素メインイメージ
            buttonMainImage.HorizontalAlignment = HorizontalAlignment.Left;
            buttonMainImage.Height = 200;
            buttonMainImage.Width = Double.NaN;
            buttonMainImage.Margin = new Thickness(15, 5, 0, 0);

            // リンク要素タイトル
            textboxTitle.FontSize = 20;
            textboxTitle.HorizontalAlignment = HorizontalAlignment.Left;
            textboxTitle.TextAlignment = TextAlignment.Left;
            textboxTitle.MinWidth = 300;
            textboxTitle.Margin = new Thickness(15, 5, 0, 0);
            
            // リンク要素メモ
            textMemo.HorizontalAlignment = HorizontalAlignment.Left;
            textMemo.MinWidth = 300;
            textMemo.Height = Double.NaN;
            textMemo.AcceptsReturn = true;
            textMemo.TextWrapping = TextWrapping.Wrap;
            textMemo.VerticalContentAlignment = VerticalAlignment.Top;
            textMemo.Margin = new Thickness(25, 15, 0, 0);

            // リンク選択
            buttonLink.Margin = new Thickness(25, 15, 0, 0);

            // リンクパス表示
            //textblockFullPath

            // ファイル開くボタン
            buttonStarting.Width = 100;
            buttonStarting.Height = 40;
            buttonStarting.HorizontalAlignment = HorizontalAlignment.Left;
            buttonStarting.Margin = new Thickness(50, 40, 0, 0);

            // フォルダ開くボタン
            buttonOpenFolder.Width = 100;
            buttonOpenFolder.Height = 40;
            buttonOpenFolder.HorizontalAlignment = HorizontalAlignment.Left;
            buttonOpenFolder.Margin = new Thickness(50, 10, 0, 0);

            // リンク要素編集欄
            stackEditElementLink.Width = Const.WidthStackEditElementLink;


            //*************************************************
            // 特別編集部
            //*************************************************
            //dockBody.Children.Remove(stackEditSpecial);
            //stackEditSpecial

            //*************************************************
            // 本体
            //*************************************************
            // this.MaxWidth = CalculationMaxWidthThisForm();
            //  this.Width = this.MaxWidth;
            this.Height = 600;


            //*************************************************
            // 全体カラー設定
            //*************************************************
            SettingColor(ColorSet);

        }

        /// <summary>
        /// カラー設定(＆画像設定)
        /// </summary>
        private void SettingColor(ColorSet ColorSet)
        {
            ColorToolThis color = new ColorToolThis(ColorSet);


            //*************************************************
            // 下地
            //*************************************************
            dockBody.Background = color.ColorBackForm;


            //*************************************************
            // メニュー部
            //*************************************************

            // メニュー 背景色
            stackMenu.Background = color.ColorBackMenu;

            // メニュー オプション設定ボタン 背景色
            buttonSettingEnvironment.Background = new SolidColorBrush(Colors.Transparent);
            buttonSettingEnvironment.BorderBrush = new SolidColorBrush(Colors.Transparent);

            // メニュー オプション設定ボタン アイコン設定
            Image imageButtonSettingEnvironment = (Image)buttonSettingEnvironment.Content;
            imageButtonSettingEnvironment.Source = new BitmapImage(new Uri("/LauncherM;component/resource/ei-settings_White.png", UriKind.Relative));

            // メニュー 表示非表示ボタン
            if(stackMenu.Visibility == Visibility.Visible)
            {
                buttonDispMenu.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispMenu.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispMenu.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispMenu.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            if (stackLauncher00.Visibility == Visibility.Visible)
            {
                buttonDispLauncher00.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher00.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispLauncher00.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher00.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            if (stackLauncher01.Visibility == Visibility.Visible)
            {
                buttonDispLauncher01.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher01.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispLauncher01.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher01.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            if (stackLauncher02.Visibility == Visibility.Visible)
            {
                buttonDispLauncher02.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher02.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispLauncher02.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher02.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            if (stackLauncher03.Visibility == Visibility.Visible)
            {
                buttonDispLauncher03.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispLauncher03.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispLauncher03.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispLauncher03.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                buttonDispEditElementLink.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispEditElementLink.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispEditElementLink.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispEditElementLink.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                buttonDispEditSpecial.Background = color.ColorBackOnButtonVisible_Menu;
                buttonDispEditSpecial.BorderBrush = color.ColorBorderOnButtonVisible_Menu;
            }
            else
            {
                buttonDispEditSpecial.Background = color.ColorBackOffButtonVisible_Menu;
                buttonDispEditSpecial.BorderBrush = color.ColorBorderOffButtonVisible_Menu;
            }

            // メニュー 下部 仕切線 色 
            stackPartitionHorizontal.Background = color.ColorPartition;


            //*************************************************
            // ランチャー部
            //*************************************************
            Brush[] brushBackgroundLauncher = { color.ColorBackLauncher00, color.ColorBackLauncher01, color.ColorBackLauncher02, color.ColorBackLauncher03 };
            Brush[] brushBackgroundElementLink = { color.ColorBackElementLink00, color.ColorBackElementLink01, color.ColorBackElementLink02, color.ColorBackElementLink03 };
            Brush[] brushBorderBrushElementLink = { color.ColorBackLauncher00, color.ColorBackLauncher01, color.ColorBackLauncher02, color.ColorBackLauncher03 };
            Brush[] brushForegroundElementLink = { color.ColorFontBasic, color.ColorFontBasic, color.ColorFontBasic, color.ColorFontBasic };

            List<Button> listButtonElementLink;
            Button buttonElementLink;
            for (int i = 0; i < m_StackLauncher.Count; i++)
            {
                // ランチャー背景の色
                if (i < brushBackgroundLauncher.Length)
                {
                    m_StackLauncher[i].Background = brushBackgroundLauncher[i];
                }
                else
                {
                    m_StackLauncher[i].Background = brushBackgroundLauncher.Last();
                }

                // リンク要素の色
                listButtonElementLink = m_ListListButtonElementLink[i];
                for (int j = 0; j < listButtonElementLink.Count; j++)
                {
                    buttonElementLink = listButtonElementLink[j];

                    if (i < brushBackgroundLauncher.Length)
                    {
                        buttonElementLink.Background = brushBackgroundElementLink[i];
                    }
                    else
                    {
                        buttonElementLink.Background = brushBackgroundElementLink.Last();
                    }

                    if (i < brushBorderBrushElementLink.Length)
                    {
                        buttonElementLink.BorderBrush = brushBorderBrushElementLink[i];
                    }
                    else
                    {
                        buttonElementLink.BorderBrush = brushBorderBrushElementLink.Last();
                    }

                    TextBlock textblock = (TextBlock)((StackPanel)buttonElementLink.Content).Children[1];
                    if (i < brushForegroundElementLink.Length)
                    {
                        textblock.Foreground = brushForegroundElementLink[i];
                    }
                    else
                    {
                        textblock.Foreground = brushForegroundElementLink.Last();
                    }
                }
            }
       

            //*************************************************
            // 縦仕切 壱
            //*************************************************
            stackPartitionVertical01.Background = color.ColorPartition;


            //*************************************************
            // リンク要素編集部
            //*************************************************          
                      
            // リンク要素編集部 背景色
            stackEditElementLink.Background = color.ColorBackEditElementLink;

            // タイトル表示欄
            textboxTitle.Background = Brushes.Transparent;
            textboxTitle.BorderBrush = Brushes.Transparent;
            textboxTitle.Foreground = color.ColorFontBasic;
            textboxTitle.CaretBrush = textboxTitle.Foreground;

            // メインイメージ表示欄
            buttonMainImage.Background = Brushes.Transparent;
            buttonMainImage.BorderBrush = Brushes.Transparent;

            // メモ欄
            textMemo.Background = Brushes.Transparent;
            textMemo.BorderBrush = Brushes.Transparent;
            textMemo.Foreground = color.ColorFontBasic;
            textMemo.CaretBrush = textboxTitle.Foreground;

            // ファイルの場所を開くボタン
            buttonOpenFolder.Background = color.ColorBackButton_EditElementLink;
            buttonOpenFolder.BorderBrush = color.ColorBorderButton_EditElementLink;
            buttonOpenFolder.Foreground = color.ColorFontButton_EditElementLink;

            // 起動ボタン
            buttonStarting.Background = color.ColorBackButton_EditElementLink;
            buttonStarting.BorderBrush = color.ColorBorderButton_EditElementLink;
            buttonStarting.Foreground = color.ColorFontButton_EditElementLink;
            
            // リンク要素アイコン
            buttonLink.Background = Brushes.Transparent;
            buttonLink.BorderBrush = Brushes.Transparent;

            // リンク要素URL
            textblockFullPath.Background = Brushes.Transparent;
            textblockFullPath.Foreground = color.ColorFontBasic;


            //*************************************************
            // 縦仕切 弐
            //*************************************************
            stackPartitionVertical02.Background = color.ColorPartition;


            //*************************************************
            // 特別編集部
            //*************************************************

            // 背景色
            stackEditSpecial.Background = color.ColorBackEditSpecial;

            // Text
            textBlock.Background = color.ColorBackEditor_EditSpecial;
            textBlock.Foreground = color.ColorFontEditor_EditSpecial;

        }

        /// <summary>
        /// ランチャーにリンク要素を新規追加
        /// </summary>
        /// <param name="StackPanel">ランチャー</param>
        public void AddElementLinkNew(StackPanel Stack)
        {
            // ボタンを追加
            Button button = new Button();
            button.HorizontalAlignment = buttonElementLinkInit.HorizontalAlignment;
            button.Height = buttonElementLinkInit.Height;
            button.Margin = buttonElementLinkInit.Margin;
            button.VerticalAlignment = buttonElementLinkInit.VerticalAlignment;
            button.Width = buttonElementLinkInit.Width;
            button.Tag = m_ListListButtonElementLink[int.Parse(Stack.Tag.ToString())].Count;
            button.Click += buttonElementLink_Click;

            // ボタンの中にスタックを追加
            StackPanel stack = new StackPanel();
            button.Content = stack;

            // スタックの中にイメージを追加
            Image image = new Image();
            stack.Children.Add(image);

            // スタックの中にテキストブロックを追加
            TextBlock textblock = new TextBlock();
            textblock.TextWrapping = TextWrapping.Wrap;
            textblock.TextTrimming = TextTrimming.CharacterEllipsis;
            textblock.HorizontalAlignment = HorizontalAlignment.Center;
            textblock.TextAlignment = TextAlignment.Center;
            stack.Children.Add(textblock);

            // ボタンをContentControlに格納
            ContentControl contentControl = new ContentControl();
            contentControl.Content = button;

            // ボタンの中にコンテキストメニューを追加
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItemDelete = new MenuItem();
            menuItemDelete.Header = m_CharacterString.Delete[(int)m_SettingLangage];
            menuItemDelete.Click += contextMenuItemDelete_Click;
            contextMenu.Items.Add(menuItemDelete);
            button.ContextMenu = contextMenu;

            // ランチャーの中にリンク要素ボタンを追加
            Stack.Children.Add(contentControl);                                        // 作成したボタンをランチャーのスタックパネルに表示

            // リンク要素ボタンリストへデータの登録
            m_ListListButtonElementLink[int.Parse(Stack.Tag.ToString())].Add(button);        // リンク要素リストへ追加

            //*************************************************
            // 編集データリストへデータの登録
            //*************************************************
            StructureDataSavePartData thisElementLink = GetDataSaveThisElementLink(int.Parse(Stack.Tag.ToString()), int.Parse(button.Tag.ToString()), m_DataSaveEditing);
            if (thisElementLink.NoAffiliationLuncher != int.Parse(Stack.Tag.ToString())
                || thisElementLink.OrderDisplayInLuncher != int.Parse(button.Tag.ToString()))
            {// m_DataSaveEditingに今回作成したリンク要素の箱がなければ
                StructureDataSavePartData data = new StructureDataSavePartData();
                data.NoAffiliationLuncher = int.Parse(Stack.Tag.ToString());
                data.OrderDisplayInLuncher = int.Parse(button.Tag.ToString());
                m_DataSaveEditing.PartData.Add(data);
            }
            
        }


        /// <summary>
        /// 指定のリンク要素を削除
        /// </summary>
        /// <param name="StackPanel">ランチャー</param>
        public void DeleteElementLinkNew(Button Button)
        {

            //*************************************************
            // 所属ランチャーと表示順を探索
            //*************************************************            
            ContentControl contentControl = (ContentControl)Button.Parent;
            StackPanel stack = (StackPanel)contentControl.Parent;
            int noAffiliationLuncher = int.Parse(stack.Tag.ToString());
            int orderDisplayInLuncher = int.Parse(Button.Tag.ToString());

            //*************************************************
            // リンク要素ボタンリストから当該リンク要素を削除
            //*************************************************
            // ランチャーから削除
            m_StackLauncher[noAffiliationLuncher].Children.Remove(contentControl); 
            
            // リンク要素リストから削除
            m_ListListButtonElementLink[noAffiliationLuncher].RemoveAt(orderDisplayInLuncher);

            // リンク要素の番号を更新
            foreach(Button button in m_ListListButtonElementLink[noAffiliationLuncher])
            {
                if(int.Parse(button.Tag.ToString()) > orderDisplayInLuncher)
                {
                    button.Tag = int.Parse(button.Tag.ToString()) - 1;
                }
            }

            //*************************************************
            // 編集データリストからデータを削除
            //*************************************************
            // リストからデータを削除
            int noDataSave = GetNoDataSaveThisElementLink(noAffiliationLuncher, orderDisplayInLuncher, m_DataSaveEditing);
            m_DataSaveEditing.PartData.RemoveAt(noDataSave);

            // 各データのリンク要素の番号を更新
            for(int i = 0; i < m_DataSaveEditing.PartData.Count; i++)
            {
                if (m_DataSaveEditing.PartData[i].NoAffiliationLuncher == noAffiliationLuncher
                    && m_DataSaveEditing.PartData[i].OrderDisplayInLuncher > orderDisplayInLuncher)
                {
                    m_DataSaveEditing.PartData[i].OrderDisplayInLuncher -= 1; 
                }
            }

        }


        /// <summary>
        /// 環境設定を読込
        /// </summary>
        public StructureSettingsEnvironmental ReadSettingsEnvironmental()
        {

            //*************************************************
            // 戻り値の格納先を宣言
            //*************************************************
            StructureSettingsEnvironmental returnValue = new StructureSettingsEnvironmental();

            //*************************************************
            // ファイル読み込み ※一行づつリストに格納
            //*************************************************
            List<string> readText = m_GeneralPurpose.ReadTextFileOutList(Const.NameFile_SettingsEnvironmental, Const.CodeCharacter_ShiftJis);

            // 表示設定
            returnValue.DispMenu = true;
            returnValue.DispLauncher00 = true;
            returnValue.DispLauncher01 = true;
            returnValue.DispLauncher02 = true;
            returnValue.DispLauncher03 = true;
            returnValue.DispEditElementLink = true;
            returnValue.DispEditSpecial = false;
            
            if(readText.Count != 0)
            {// 環境設定が保存されている時

                //*************************************************
                // 一行づつ解析
                //*************************************************
                foreach (string Line in readText)
                {
                    // [0]シンボル と [1]値 に分ける
                    string[] element = Line.Split(':');

                    // メニューの表示設定
                    if (element[0].Equals(Const.Symbol_DispMenu))
                    {
                        returnValue.DispMenu = Convert.ToBoolean(element[1]);
                    }

                    // ランチャー00の表示設定
                    if (element[0].Equals(Const.Symbol_DispLauncher00))
                    {
                        returnValue.DispLauncher00 = bool.Parse(element[1]);
                    }

                    // ランチャー01の表示設定
                    if (element[0].Equals(Const.Symbol_DispLauncher01))
                    {
                        returnValue.DispLauncher01 = bool.Parse(element[1]);
                    }

                    // ランチャー02の表示設定
                    if (element[0].Equals(Const.Symbol_DispLauncher02))
                    {
                        returnValue.DispLauncher02 = bool.Parse(element[1]);
                    }

                    // ランチャー03の表示設定
                    if (element[0].Equals(Const.Symbol_DispLauncher03))
                    {
                        returnValue.DispLauncher03 = bool.Parse(element[1]);
                    }

                    // リンク要素編集領域の表示設定
                    if (element[0].Equals(Const.Symbol_DispEditElementLink))
                    {
                        returnValue.DispEditElementLink = bool.Parse(element[1]);
                    }

                    // 特別編集領域の表示設定
                    if (element[0].Equals(Const.Symbol_DispEditSpecial))
                    {
                        returnValue.DispEditSpecial = bool.Parse(element[1]);
                    }
                }

            }


            //*************************************************
            // 戻り値を返す
            //*************************************************
            return returnValue;

        }

        /// <summary>
        /// 環境設定を書出
        /// </summary>
        /// <param name="SubjectWrite">書き出す対象</param>
        public void WriteSettingsEnvironmental(StructureSettingsEnvironmental SubjectWrite)
        {

            //*************************************************
            // 書き出す文字列を作成
            //*************************************************
            // 宣言
            string StringToWrite;

            // 環境設定ファイルの各領域の表示設定 記録
            StringToWrite = Const.Symbol_DispMenu + ':' + SubjectWrite.DispMenu.ToString();
            StringToWrite += Environment.NewLine;
            StringToWrite += Const.Symbol_DispLauncher00 + ':' + SubjectWrite.DispLauncher00.ToString();
            StringToWrite += Environment.NewLine;
            StringToWrite += Const.Symbol_DispLauncher01 + ':' + SubjectWrite.DispLauncher01.ToString();
            StringToWrite += Environment.NewLine;
            StringToWrite += Const.Symbol_DispLauncher02 + ':' + SubjectWrite.DispLauncher02.ToString();
            StringToWrite += Environment.NewLine;
            StringToWrite += Const.Symbol_DispLauncher03 + ':' + SubjectWrite.DispLauncher03.ToString();
            StringToWrite += Environment.NewLine;
            StringToWrite += Const.Symbol_DispEditElementLink + ':' + SubjectWrite.DispEditElementLink.ToString();
            StringToWrite += Environment.NewLine;
            StringToWrite += Const.Symbol_DispEditSpecial + ':' + SubjectWrite.DispEditSpecial.ToString();
            StringToWrite += Environment.NewLine;

            //*************************************************
            // ファイル書き出し
            //*************************************************
            m_GeneralPurpose.WriteTextFile(Const.NameFile_SettingsEnvironmental, false, Const.CodeCharacter_ShiftJis, StringToWrite);
        }

        /// <summary>
        /// 編集データを読込
        /// </summary>
        public StructureDataSave ReadDataEdited()
        {

            //*************************************************
            // 戻り値の格納先を宣言
            //*************************************************
            StructureDataSave returnValue = new StructureDataSave();

            //*************************************************
            // ファイル読み込み ※一行づつリストに格納
            //*************************************************
            List<string> readText = m_GeneralPurpose.ReadTextFileOutList(Const.NameFile_DataSave, Const.CodeCharacter_ShiftJis);

            //*************************************************
            // 一行づつ解析
            //*************************************************
            if (readText.Count > 0)
            {// 何かしら記述があるとき
                for (int i = 0; i < readText.Count; i++)
                {

                    // [0]所属するランチャーの番号
                    // [1]ランチャー内の表示順序                           
                    // [2]exeファイルパス                           
                    // [3]アイコン画像パス                           
                    // [4]ビジュアル画像パス                         
                    // [5]タイトル文字列                        
                    // [6]メモ文字列                                
                    // [7],[8],… select.def(.lua)パス
                    // に分ける
                    string[] element = readText[i].Split(':');

                    StructureDataSavePartData returnValueItem = new StructureDataSavePartData();
                    ushort number = 0;

                    returnValueItem.NoAffiliationLuncher = int.Parse(element[number]);      // 所属するランチャーの番号       
                    number++;
                    returnValueItem.OrderDisplayInLuncher = int.Parse(element[number]);     // ランチャー内の表示順序           
                    number++;
                    returnValueItem.PathExe = RestoreCharacter(element[number]);            // exeファイルパス                 
                    number++;
                    returnValueItem.PathImageIcon = RestoreCharacter(element[number]);      // アイコン画像パス            
                    number++;
                    returnValueItem.PathImagevisual = RestoreCharacter(element[number]);    // ビジュアル画像パス       
                    number++;
                    returnValueItem.StringTitle = RestoreCharacter(element[number]);        // タイトル文字列        
                    number++;
                    returnValueItem.StringMemo = RestoreCharacter(element[number]);         // メモ文字列         
                    number++;

                    for (int j = number; j < element.Count(); j++)                          // select.def(.lua)パス   
                    {
                        returnValueItem.PathFileSelect.Add(RestoreCharacter(element[number]));
                        number++;
                    }

                    returnValue.PartData.Add(returnValueItem);
                    
                }
            }

            //*************************************************
            // 戻り値を返す
            //*************************************************
            return returnValue;
        }

        /// <summary>
        /// 編集データを書出
        /// </summary>
        public void WriteDataEdited(StructureDataSave SubjectWrite)
        {

            //*************************************************
            // 書き出す文字列の格納変数を宣言
            //*************************************************
            string StringToWrite = string.Empty;

            //*************************************************
            // リンク要素分(全ランチャー分の合計)ループ
            //*************************************************
            for (int i = 0; i < SubjectWrite.PartData.Count; i++)
            {
                // 1リンク枠分のデータを1行の文字列にする
                StringToWrite = StringToWrite + SubjectWrite.PartData[i].NoAffiliationLuncher.ToString()
                                + ':' + SubjectWrite.PartData[i].OrderDisplayInLuncher.ToString()
                                + ':' + ReplacementCharacter(SubjectWrite.PartData[i].PathExe)
                                + ':' + ReplacementCharacter(SubjectWrite.PartData[i].PathImageIcon)
                                + ':' + ReplacementCharacter(SubjectWrite.PartData[i].PathImagevisual)
                                + ':' + ReplacementCharacter(SubjectWrite.PartData[i].StringTitle)
                                + ':' + ReplacementCharacter(SubjectWrite.PartData[i].StringMemo);

                if (SubjectWrite.PartData[i].PathFileSelect != null)
                {
                    for (int j = 0; j < SubjectWrite.PartData[i].PathFileSelect.Count; j++)
                    {
                        StringToWrite = StringToWrite
                                    + ':' + ReplacementCharacter(SubjectWrite.PartData[i].PathFileSelect[j]);
                    }
                }

                StringToWrite += Environment.NewLine;
            }
            
            //*************************************************
            // ファイル書き出し
            //*************************************************
            m_GeneralPurpose.WriteTextFile(Const.NameFile_DataSave, false, Const.CodeCharacter_ShiftJis, StringToWrite);

        }
        
        /// <summary>
        /// ファイルへ書き出す文字列中の禁止文字を置き換え
        /// </summary>
        public string ReplacementCharacter(string String)
        {
            if(String == null)
            {
                return String;
            }

            string returnValue = String;
            returnValue = returnValue.Replace(":", Const.CharacterReplacement_Colon);
            returnValue = returnValue.Replace("\r", Const.CharacterReplacement_CarriagReturn);
            returnValue = returnValue.Replace("\n", Const.CharacterReplacement_LineFeed);
            return returnValue;
        }
        
        /// <summary>
        /// ファイルから読み込んだ文字列の復元
        /// </summary>
        public string RestoreCharacter(string String)
        {
            if (String == null)
            {
                return String;
            }

            string returnValue = String;
            returnValue = returnValue.Replace(Const.CharacterReplacement_Colon, ":");
            returnValue = returnValue.Replace(Const.CharacterReplacement_CarriagReturn, "\r");
            returnValue = returnValue.Replace(Const.CharacterReplacement_LineFeed, "\n");
            return returnValue;
        }


        /// <summary>
        /// リンク要素の所属するランチャー番号,表示順序から，そのリンク要素の保存データを取得
        /// </summary>
        public StructureDataSavePartData GetDataSaveThisElementLink(int NoAffiliationLuncher, int OrderDisplayInLuncher, StructureDataSave DataSave)
        {            

            // ランチャー番号の一致する編集データを抽出
            List<StructureDataSavePartData> dataMatchNoAffiliationLuncher = new List<StructureDataSavePartData>();
            foreach (StructureDataSavePartData data in DataSave.PartData)
            {
                // 抽出データの並び順番号を確認
                if (NoAffiliationLuncher == data.NoAffiliationLuncher)
                {
                    dataMatchNoAffiliationLuncher.Add(data);
                }
            }

            // リンク要素表示順序の一致する編集データを抽出
            StructureDataSavePartData dataMatch = new StructureDataSavePartData();
            foreach (StructureDataSavePartData data in dataMatchNoAffiliationLuncher)
            {
                // 抽出データの並び順番号を確認
                if (OrderDisplayInLuncher == data.OrderDisplayInLuncher)
                {
                    dataMatch = data;
                    break;
                }
            }

            // 結果を返す
            return dataMatch;

        }


        /// <summary>
        /// リンク要素の所属するランチャー番号,表示順序から，そのリンク要素の保存データの要素番号を取得
        /// </summary>
        public int GetNoDataSaveThisElementLink(int NoAffiliationLuncher, int OrderDisplayInLuncher, StructureDataSave DataSave)
        {

            int i;
            for(i = 0; i < DataSave.PartData.Count; i++)
            {
                if(DataSave.PartData[i].NoAffiliationLuncher == NoAffiliationLuncher
                    && DataSave.PartData[i].OrderDisplayInLuncher == OrderDisplayInLuncher)
                {
                    break;
                }
            }
            
            // 結果を返す
            return i;

        }


        /// <summary>
        /// PartData内の，指定ランチャーのリンク要素個数を数える
        /// </summary>
        /// <param name="StructureDataSave">リンク要素の一覧</param>
        /// <param name="NoAffiliationLuncher">ランチャー番号</param>
        private int CountElementLinkInPartData(StructureDataSave StructureDataSave, int NoAffiliationLuncher)
        {
            int count = 0;

            foreach (StructureDataSavePartData data in StructureDataSave.PartData)
            {
                // 抽出データの並び順番号を確認
                if (NoAffiliationLuncher == data.NoAffiliationLuncher)
                {
                    count++;
                }
            }

            return count;
        }



        /// <summary>
        /// 画面最大幅を計算
        /// </summary>
        public double CalculationMaxWidthThisForm()
        {

            //*************************************************
            // 戻り値の格納先を宣言
            //*************************************************
            double returnValue = 0;


            //*************************************************
            // 計算
            //*************************************************

            // ランチャー分足す
            for(int i = 0; i < m_StackLauncher.Count; i++)
            {
                if (m_StackLauncher[i].Visibility == Visibility.Visible)
                {
                    returnValue += m_StackLauncher[i].Width;
                }
            }

            // 要素編集部分足す
            if (stackEditElementLink.Visibility == Visibility.Visible)
            {
                returnValue += stackEditElementLink.Width;
            }

            // 特別編集部分足す
            if (stackEditSpecial.Visibility == Visibility.Visible)
            {
                returnValue += stackEditSpecial.Width;
            }

            // フレーム分足す
            returnValue += (this.ActualWidth - dockMain.ActualWidth);
            

            //*************************************************
            // 戻り値を返す
            //*************************************************
            return returnValue;

        }

    }

}
    
