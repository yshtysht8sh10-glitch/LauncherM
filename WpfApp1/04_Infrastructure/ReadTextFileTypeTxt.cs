using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LauncherM._04_Infrastructure
{

    public class ReadTextFileTypeTxt : IReadFileText
    {
        string pathFile { get; set; }

        string characterCode { get; set; }

        public ReadTextFileTypeTxt(string pathFile, string characterCode)
        {
            this.pathFile = pathFile;
            this.characterCode = characterCode;
        }

        /// <summary>
        /// テキストファイル読込 (1つの文字列として返す)
        /// </summary>
        /// <returns>読み込んだテキストファイルのテキスト (1つの文字列として返す)</returns>
        public string ReadTextAll()
        {
            try
            {
                using StreamReader sr = new StreamReader(pathFile, Encoding.GetEncoding(characterCode));
                string str = sr.ReadToEnd();
                sr.Close();

                return str;
            }
            catch
            {// ファイル読込に失敗した時はnullを返す
                System.Windows.Forms.MessageBox.Show("ファイル読み込みに失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// テキストファイル読込 (1行づつリストに格納して返す)
        /// </summary>
        /// <returns>読み込んだテキストファイルのテキスト (1行づつリストに格納して返す)</returns>
        public List<string> ReadTextOneLineList()
        {
            try
            {
                using StreamReader sr = new StreamReader(pathFile, Encoding.GetEncoding(characterCode));
                List<string> returnString = new List<string>();
                while (sr.Peek() > -1)
                {
                    returnString.Add(sr.ReadLine());
                }
                sr.Close();

                return returnString;
            }
            catch
            {// ファイル読込に失敗した時はnullを返す
                System.Windows.Forms.MessageBox.Show("ファイル読み込みに失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }
    }
}
