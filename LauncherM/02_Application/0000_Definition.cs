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
    /// 列挙体__言語
    /// </summary>
    public enum ItemLangage
    {
        Japanese = 0,
        English,
    }

    /// <summary>
    /// 列挙体__カラーセット
    /// </summary>
    public enum ColorSet 
    {
        Dark01 = 0,
    }

    /// <summary>
    /// 定数
    /// </summary>
    class Const
    {

        // ファイル名
        public const string NameFile_SettingsEnvironmental = "se.txt";     // 環境設定ファイルのファイル名
        public const string NameFile_DataSave = "de.txt";                // 編集データ保存ファイルのファイル名

        // データシンボル
        public const string Symbol_DispMenu = "DMNU";               // メニューの表示設定を表すシンボル
        public const string Symbol_DispLauncher00 = "DL00";         // ランチャー00の表示設定を表すシンボル
        public const string Symbol_DispLauncher01 = "DL01";         // ランチャー01の表示設定を表すシンボル
        public const string Symbol_DispLauncher02 = "DL02";         // ランチャー02の表示設定を表すシンボル
        public const string Symbol_DispLauncher03 = "DL03";         // ランチャー03の表示設定を表すシンボル
        public const string Symbol_DispEditElementLink = "DEEL";    // リンク要素編集領域の表示設定を表すシンボル
        public const string Symbol_DispEditSpecial = "DESP";        // 特別編集領域の表示設定を表すシンボル

        // 文字コード
        public const string CodeCharacter_ShiftJis = "Shift_JIS";           // 文字コード：Shift_JIS
        public const string CodeCharacter_Utf8 = "utf-8";                   // 文字コード：utf-8

        // 置き換え文字
        public const string CharacterReplacement_Colon = "###ColonColonColon";          // 『：』
        public const string CharacterReplacement_CarriagReturn = "###CarriCarriCarri";  // 『\r』
        public const string CharacterReplacement_LineFeed = "###LinefLinefnLinef";      // 『\n』

        // サイズ
        public const int WidthStackLauncher = 80;                       // ランチャーの幅
        public const int WidthStackEditElementLink = 350;               // リンク要素編集部の幅

        // 他
        public const int MaxQuantityStackLauncher = 4;              // ランチャーの最大個数

    }

    /// <summary>
    /// 言語別の文字列
    /// </summary>
    class CharacterString
    {

        public string[] SelectLinkFile = new string[]
        {
            "リンクするファイルを選択",
            "Select the file to link",
        };

        public string[] SelectAnImage = new string[]
        {
            "画像を選択",          
            "Select an image",      
        };
        
        public string[] Addition = new string[]
        {
            "追加",             
            "addition",         
        };
        
        public string[] Delete = new string[]
        {
            "削除",             
            "Delete",                
        };

        public string[] DispMenu = new string[]
        {
            "メニュー表示/非表示",
            "menu display/Hide ",
        };

    }

    /// <summary>
    /// 構造体的に使用するクラス__環境設定
    /// </summary>
    /// <remarks> ※環境設定ファイルに読み書きするデータはこのクラスに格納する </remarks>
    public class StructureSettingsEnvironmental
    {

        // 各領域の表示設定
        public bool DispMenu;  
        public bool DispLauncher00;
        public bool DispLauncher01;
        public bool DispLauncher02;
        public bool DispLauncher03;
        public bool DispEditElementLink;
        public bool DispEditSpecial;

    }


    /// <summary>
    /// 色
    /// </summary>
    class ColorToolThis
    {
        public Brush ColorFontBasic;                       // 基本文字 色            
        public Brush ColorPartition;                       // 仕切線 色            
        public Brush ColorBackForm;                        // Form 背景色            
        public Brush ColorBackMenu;                        // メニュー 背景色            
        public Brush ColorBackOnButtonVisible_Menu;        // メニュー 表示非表示ボタン(点灯時) 背景色            
        public Brush ColorBorderOnButtonVisible_Menu;      // メニュー 表示非表示ボタン(点灯時) ボーダー色            
        public Brush ColorBackOffButtonVisible_Menu;       // メニュー 表示非表示ボタン(消灯時) 背景色            
        public Brush ColorBorderOffButtonVisible_Menu;     // メニュー 表示非表示ボタン(消灯時) ボーダー色            
        public Brush ColorBackLauncher00;                  // ランチャー00 背景色            
        public Brush ColorBackLauncher01;                  // ランチャー01 背景色            
        public Brush ColorBackLauncher02;                  // ランチャー02 背景色            
        public Brush ColorBackLauncher03;                  // ランチャー03 背景色                 
        public Brush ColorBackEditElementLink;              // リンク要素編集部 背景色      
        public Brush ColorBackButton_EditElementLink;      // リンク要素編集部 ボタン 背景色            
        public Brush ColorBorderButton_EditElementLink;    // リンク要素編集部 ボタン ボーダー色            
        public Brush ColorFontButton_EditElementLink;      // リンク要素編集部 ボタン 文字色            
        public Brush ColorBackEditor_EditElementLink;      // リンク要素編集部 入力欄 背景色            
        public Brush ColorBorderEditor_EditElementLink;    // リンク要素編集部 入力欄 ボーダー色            
        public Brush ColorFontEditor_EditElementLink;      // リンク要素編集部 入力欄 文字色   
        public Brush ColorBackElementLink00;                // リンク要素 背景色            
        public Brush ColorBackElementLink01;                // リンク要素 背景色       
        public Brush ColorBackElementLink02;                // リンク要素 背景色       
        public Brush ColorBackElementLink03;                // リンク要素 背景色             
        public Brush ColorBackEditSpecial;                 // 特別編集部 背景色            
        public Brush ColorBackButton_EditSpecial;          // 特別編集部 ボタン 背景色            
        public Brush ColorBorderButton_EditSpecial;        // 特別編集部 ボタン ボーダー色            
        public Brush ColorFontButton_EditSpecial;          // 特別編集部 ボタン 文字色            
        public Brush ColorBackEditor_EditSpecial;          // 特別編集部 入力欄 背景色            
        public Brush ColorBorderEditor_EditSpecial;        // 特別編集部 入力欄 ボーダー色            
        public Brush ColorFontEditor_EditSpecial;          // 特別編集部 入力欄 文字色

        public ColorToolThis(ColorSet colorSet)
        {
            BrushConverter converter = new BrushConverter();

            switch (colorSet)
            {
                case ColorSet.Dark01:

                    // 基本文字 色
                    ColorFontBasic = (Brush)converter.ConvertFromString("#" + 255.ToString("x2") + 255.ToString("x2") + 255.ToString("x2"));

                    // 仕切線 色
                    ColorPartition = (Brush)converter.ConvertFromString("#" + 220.ToString("x2") + 220.ToString("x2") + 220.ToString("x2"));

                    // Form 背景色
                    ColorBackForm = (Brush)converter.ConvertFromString("#" + 0.ToString("x2") + 0.ToString("x2") + 0.ToString("x2"));

                    // メニュー 背景色
                    ColorBackMenu = (Brush)converter.ConvertFromString("#" + 150.ToString("x2") + 150.ToString("x2") + 150.ToString("x2"));

                    // メニュー 表示非表示ボタン(点灯時) 背景色
                    ColorBackOnButtonVisible_Menu = (Brush)converter.ConvertFromString("#" + 100.ToString("x2") + 100.ToString("x2") + 255.ToString("x2"));

                    // メニュー 表示非表示ボタン(点灯時) ボーダー色
                    ColorBorderOnButtonVisible_Menu = (Brush)converter.ConvertFromString("#" + 10.ToString("x2") + 10.ToString("x2") + 60.ToString("x2"));

                    // メニュー 表示非表示ボタン(消灯時) 背景色
                    ColorBackOffButtonVisible_Menu = (Brush)converter.ConvertFromString("#" + 0.ToString("x2") + 0.ToString("x2") + 155.ToString("x2"));

                    // メニュー 表示非表示ボタン(消灯時) ボーダー色
                    ColorBorderOffButtonVisible_Menu = (Brush)converter.ConvertFromString("#" + 0.ToString("x2") + 0.ToString("x2") + 30.ToString("x2"));

                    // ランチャー00 背景色
                    int launcher00_R = 130;
                    int launcher00_G = 130;
                    int launcher00_B = 130;
                    ColorBackLauncher00 = (Brush)converter.ConvertFromString("#" + launcher00_R.ToString("x2") + launcher00_G.ToString("x2") + launcher00_B.ToString("x2"));

                    // ランチャー01 背景色
                    int launcher01_R = 110;
                    int launcher01_G = 110;
                    int launcher01_B = 110;
                    ColorBackLauncher01 = (Brush)converter.ConvertFromString("#" + launcher01_R.ToString("x2") + launcher01_G.ToString("x2") + launcher01_B.ToString("x2"));

                    // ランチャー02 背景色
                    int launcher02_R = 90;
                    int launcher02_G = 90;
                    int launcher02_B = 90;
                    ColorBackLauncher02 = (Brush)converter.ConvertFromString("#" + launcher02_R.ToString("x2") + launcher02_G.ToString("x2") + launcher02_B.ToString("x2"));

                    // ランチャー03 背景色
                    int launcher03_R = 70;
                    int launcher03_G = 70;
                    int launcher03_B = 70;
                    ColorBackLauncher03 = (Brush)converter.ConvertFromString("#" + launcher03_R.ToString("x2") + launcher03_G.ToString("x2") + launcher03_B.ToString("x2"));

                    // リンク要素編集部 背景色
                    ColorBackEditElementLink = (Brush)converter.ConvertFromString("#" + 50.ToString("x2") + 50.ToString("x2") + 50.ToString("x2"));

                    double parsentColorFromLauncher = 0.9;  // ランチャーに対する薄さの割合
                    ColorBackElementLink00 = (Brush)converter.ConvertFromString("#" + ((int)(launcher00_R * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher00_G * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher00_B * parsentColorFromLauncher)).ToString("x2"));
                    ColorBackElementLink01 = (Brush)converter.ConvertFromString("#" + ((int)(launcher01_R * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher01_G * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher01_B * parsentColorFromLauncher)).ToString("x2"));
                    ColorBackElementLink02 = (Brush)converter.ConvertFromString("#" + ((int)(launcher02_R * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher02_G * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher02_B * parsentColorFromLauncher)).ToString("x2"));
                    ColorBackElementLink03 = (Brush)converter.ConvertFromString("#" + ((int)(launcher03_R * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher03_G * parsentColorFromLauncher)).ToString("x2") + ((int)(launcher03_B * parsentColorFromLauncher)).ToString("x2"));

                    // リンク要素編集部 ボタン 背景色
                    ColorBackButton_EditElementLink = (Brush)converter.ConvertFromString("#" + 110.ToString("x2") + 110.ToString("x2") + 110.ToString("x2"));

                    // リンク要素編集部 ボタン ボーダー色
                    ColorBorderButton_EditElementLink = (Brush)converter.ConvertFromString("#" + 220.ToString("x2") + 220.ToString("x2") + 220.ToString("x2"));

                    // リンク要素編集部 ボタン 文字色
                    ColorFontButton_EditElementLink = (Brush)converter.ConvertFromString("#" + 255.ToString("x2") + 255.ToString("x2") + 255.ToString("x2"));

                    // リンク要素編集部 入力欄 背景色
                    ColorBackEditor_EditElementLink = (Brush)converter.ConvertFromString("#" + 0.ToString("x2") + 0.ToString("x2") + 0.ToString("x2"));

                    // リンク要素編集部 入力欄 ボーダー色
                    ColorBorderEditor_EditElementLink = (Brush)converter.ConvertFromString("#" + 30.ToString("x2") + 30.ToString("x2") + 30.ToString("x2"));

                    // リンク要素編集部 入力欄 文字色
                    ColorFontEditor_EditElementLink = (Brush)converter.ConvertFromString("#" + 255.ToString("x2") + 255.ToString("x2") + 255.ToString("x2"));

                    // 特別編集部 背景色
                    ColorBackEditSpecial = (Brush)converter.ConvertFromString("#" + 30.ToString("x2") + 30.ToString("x2") + 30.ToString("x2"));

                    // 特別編集部 ボタン 背景色
                    ColorBackButton_EditSpecial = (Brush)converter.ConvertFromString("#" + 110.ToString("x2") + 110.ToString("x2") + 110.ToString("x2"));

                    // 特別編集部 ボタン ボーダー色
                    ColorBorderButton_EditSpecial = (Brush)converter.ConvertFromString("#" + 220.ToString("x2") + 220.ToString("x2") + 220.ToString("x2"));

                    // 特別編集部 ボタン 文字色
                    ColorFontButton_EditSpecial = (Brush)converter.ConvertFromString("#" + 255.ToString("x2") + 255.ToString("x2") + 255.ToString("x2"));

                    // 特別編集部 入力欄 背景色
                    ColorBackEditor_EditSpecial = (Brush)converter.ConvertFromString("#" + 0.ToString("x2") + 0.ToString("x2") + 0.ToString("x2"));

                    // 特別編集部 入力欄 ボーダー色
                    ColorBorderEditor_EditSpecial = (Brush)converter.ConvertFromString("#" + 30.ToString("x2") + 30.ToString("x2") + 30.ToString("x2"));

                    // 特別編集部 入力欄 文字色
                    ColorFontEditor_EditSpecial = (Brush)converter.ConvertFromString("#" + 255.ToString("x2") + 255.ToString("x2") + 255.ToString("x2"));

                    break;
            }
        }

    }

}
