using System.Collections.Generic;
using Framework.Config;
using SsitEngine.Data;
using Table;

namespace Framework.Logic
{
    public class DataAccidentProxy : DataProxy<AccidentTableModel>
    {
        public DataAccidentProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public AccidentTableModel AccidentModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
        }

        //查询对应表格的数据
        public AccidentDefine GetTableData( int accidentID )
        {
            return AccidentModel.GetAccidentByAccidentID(accidentID);
        }

        #region 子类实现

        public override void OnRegister()
        {
            base.OnRegister();
            SyncLoadAndInitModel(EnTableType.AccidentTable.ToString(), true);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            mModels = null;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="id">数据id</param>
        /// <returns>对应数据id的元数据</returns>
        public override T GetData<T>( int id )
        {
            return AccidentModel.GetAccidentByAccidentID(id) as T;
        }

        public override void UpdateData( List<DataBase> dataList )
        {
        }

        #endregion
    }
}