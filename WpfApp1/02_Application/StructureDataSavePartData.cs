using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherM._02_Application
{

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
}
