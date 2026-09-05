using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherM._02_Application
{
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

}
