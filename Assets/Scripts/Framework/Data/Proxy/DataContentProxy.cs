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
    public class DataContentProxy : DataProxy<ContentTableModel>
    {
        public DataContentProxy( IDataManager dataManager ) : base(dataManager)
        {
        }

        public ContentTableModel ContentModel => mModels[JsonLoader.DefaultComponeyName];

        #region 子类继承

        public override void OnRegister()
        {
            base.OnRegister();
            //m_contentModel = LoadAndInitModel<ContentTableModel>(EnTableType.ContentTable.ToString());
            SyncLoadAndInitModel(EnTableType.ContentTable.ToString(), false);
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
            return ContentModel.GetContentByIndex(id) as T;
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
            return ContentModel.GetContentByIndex(index).content;
        }

        public static string GetTipContent( EnText id )
        {
            var define = DataManager.Instance.GetData<ContentDefine>((int) EnLocalDataType.DATA_STRING, (int) id);
            if (define != null) return define.content;
            return string.Empty;
        }

        #endregion
    }
}