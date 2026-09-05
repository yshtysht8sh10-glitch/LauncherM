using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace LauncherM
{

    /// <summary>
    /// 汎用クラス(本ツールと結びつきのない機能をまとめたクラス)
    /// </summary>
    class GeneralPurpose
    {
        [DllImport("shell32.dll", EntryPoint = "ExtractAssociatedIcon")]
        private static extern IntPtr ExtractAssociatedIcon
        (
            IntPtr hInst,
            [MarshalAs(UnmanagedType.LPStr)] string lpIconPath,
            ref int lpiIcon
        );

        [DllImport("user32.dll")]
        private static extern long DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GeneralPurpose()
        {
        }

        /// <summary>
        /// テキストファイル読込 (1つの文字列として返す)
        /// </summary>
        /// <param name="PathFile">読み込むファイルのパス</param>
        /// <param name="CharacterCode">文字コード</param>
        /// <returns>読み込んだテキストファイルのテキスト (1つの文字列として返す)</returns>
        public string ReadTextFileOutString(string PathFile, string CharacterCode)
        {

            StreamReader sr = new StreamReader(PathFile, Encoding.GetEncoding(CharacterCode));
            string str = sr.ReadToEnd();
            sr.Close();

            return str;

        }

        /// <summary>
        /// テキストファイル読込 (1行づつリストに格納して返す)
        /// </summary>
        /// <param name="PathFile">読み込むファイルのパス</param>
        /// <param name="CharacterCode">文字コード</param>
        /// <returns>読み込んだテキストファイルのテキスト (1行づつリストに格納して返す)</returns>
        public List<string> ReadTextFileOutList(string PathFile, string CharacterCode)
        {

            StreamReader sr = new StreamReader(PathFile, Encoding.GetEncoding(CharacterCode));
            List<string> returnString = new List<string>();
            while (sr.Peek() > -1)
            {
                returnString.Add(sr.ReadLine());
            }
            sr.Close();

            return returnString;

        }


        /// <summary>
        /// テキストファイル書込
        /// </summary>
        /// <param name="PathFile">書き込むファイルのパス</param>
        /// <param name="Append">true=追加 / false=上書き </param>
        /// <param name="CharacterCode">文字コード</param>
        /// <param name="StringToWrite">書き込む文字列</param>
        /// <returns>-</returns>
        /// <remarks>ファイルが存在しない時は新しいファイルを作成する</remarks>
        public void WriteTextFile(string PathFile, bool Append, string CharacterCode, string StringToWrite)
        {

            StreamWriter sw = new StreamWriter(PathFile
                                               , Append
                                               , Encoding.GetEncoding(CharacterCode));

            sw.Write(StringToWrite);

            sw.Close();

        }


        /// <summary>
        /// 指定のファイルパスからアイコンを取得
        /// </summary>
        /// <param name="PathFile">ファイルパス</param>
        /// <returns>アイコン</returns>
        public BitmapSource GetIconFromFilePathSpecified(string PathFile)
        {

            IntPtr hInst = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);
            int lpiIcon = 0;

            IntPtr hIcon = ExtractAssociatedIcon(hInst, PathFile, ref lpiIcon);

            try
            {
                //var icon = Icon.FromHandle(hIcon);
                //return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DestroyIcon(hIcon);
            }

        }


    }


}
