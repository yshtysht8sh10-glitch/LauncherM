using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LauncherM.Function
{
    internal class WriteTextFile
    {

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
            try
            {
                StreamWriter sw = new StreamWriter(PathFile
                                                   , Append
                                                   , Encoding.GetEncoding(CharacterCode));

                sw.Write(StringToWrite);

                sw.Close();
            }
            catch
            {// ファイル書込に失敗した時はメッセージを返す
                System.Windows.Forms.MessageBox.Show("ファイル書き込みに失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
