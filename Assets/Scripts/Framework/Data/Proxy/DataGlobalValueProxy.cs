/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 20:20:25                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Data;
using SsitEngine.Unity.Data;
using Table;

namespace Framework.Logic
{
    public class DataGlobalValueProxy : DataProxy<GlobalValueTableModel>
    {
        public DataGlobalValueProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public GlobalValueTableModel ContentModel => mModels[JsonLoader.DefaultComponeyName];

        #region 子类继承

        public override void OnRegister()
        {
            base.OnRegister();
            //m_contentModel = LoadAndInitModel<GlobalValueTableModel>(EnTableType.GlobalValueTable.ToString());
            SyncLoadAndInitModel(EnTableType.GlobalValueTable.ToString(), false);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            mModels.Clear();
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
            return ContentModel.GetGlobalValueByIndex(id) as T;
        }

        public override void UpdateData( List<DataBase> dataList )
        {
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// 获取对应索引的文本
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public string GetContext( int index )
        {
            return ContentModel.GetGlobalValueByIndex(index).content;
        }

        public static string GetGlobalValue( EnGlobalValue id )
        {
            var define =
                DataManager.Instance.GetData<GlobalValueDefine>((int) EnLocalDataType.DATA_GLOABLVALUE, (int) id);
            if (define != null) return define.content;
            return string.Empty;
        }

        #endregion
    }
}