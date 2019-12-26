/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：litong                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/10 18:59:22                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using Framework.Config;
using SsitEngine.Data;
using Table;

namespace Framework.Logic
{
    /// <summary>
    /// 数据代理（可以处理多个数据模型）
    /// </summary>
    public class DataEmergencyModelProxy : DataProxy<EmergencyPowerTableModel>
    {
        //todo:1写入自己代理的源数据模型列表（因其数据模型有多企业性质所以是个列表）

        public DataEmergencyModelProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public EmergencyPowerTableModel EmergencyPowerModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
        }

        public EmergencyPowerDefine GetTableData( int tableID )
        {
            return EmergencyPowerModel.GetEmergencyPowerById(tableID);
        }

        #region Internal Members

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">指定数据类型</typeparam>
        /// <param name="id">数据id</param>
        /// <returns>对应数据id的元数据</returns>
        public override T GetData<T>( int id )
        {
            return EmergencyPowerModel.GetEmergencyPowerById(id) as T;
        }

        #endregion

        public List<EmergencyPowerDefine> GetCanEditorModelByType( int type )
        {
            return EmergencyPowerModel.EmergencyPower.Where(x => x.ModelTypeID == type).ToList();
        }


        #region 子类继承

        /// <summary>
        /// 更新数据（一般用于服务器的动态数据绑定）
        /// </summary>
        /// <param name="dataList"></param>
        public override void UpdateData( List<DataBase> dataList )
        {
        }

        /// <summary>
        /// 代理注册
        /// </summary>
        public override void OnRegister()
        {
            base.OnRegister();
            SyncLoadAndInitModel(EnTableType.EmergencyPowerTable.ToString(), true);
        }

        /// <summary>
        /// 代理移除
        /// </summary>
        public override void OnRemove()
        {
            base.OnRemove();
            mModels = null;
        }

        #endregion
    }
}