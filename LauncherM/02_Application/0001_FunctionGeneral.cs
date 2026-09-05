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
