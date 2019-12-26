using System.Collections.Generic;
using Framework.Config;
using SsitEngine.Data;
using Table;

namespace Framework.Logic
{
    public class DataInvEquipProxy : DataProxy<InvEquipTableModel>
    {
        public DataInvEquipProxy( IDataManager dataManager ) : base(dataManager)
        {
        }


        public InvEquipTableModel InvEquipModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
        }

        #region 数据查询接口

        //查询对应表格的数据
        public InvEquipMentDefine GetTableData( int equipId )
        {
            return InvEquipModel.GetInvEquipMentByEqiupID(equipId);
        }

        #endregion

        #region 继承

        public override void OnRegister()
        {
            base.OnRegister();

            //mModelTable = LoadAndInitModel<InvEquipTableModel>(EnTableType.InvEquipTable.ToString(),true);
            SyncLoadAndInitModel(EnTableType.InvEquipTable.ToString(), true);
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
            return InvEquipModel.GetInvEquipMentByEqiupID(id) as T;
        }

        public override void UpdateData( List<DataBase> dataList )
        {
        }

        #endregion
    }
}