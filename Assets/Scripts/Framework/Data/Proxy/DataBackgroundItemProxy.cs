/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：后台物体代理  用具读写场景右侧界面信息                                                    
*│　作    者：qyy                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/11/27 10:42:10                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.Config;
using SsitEngine.Data;
using Table;

namespace Framework.Logic
{
    /// <summary>
    /// 数据代理（可以处理多个数据模型）
    /// </summary>
    public class DataBackgroundItemProxy : DataProxy<BackgroundItemTableModel>
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dataManager"></param>
        /// <remarks>不想写静态的代理名称可以使用表名作为代理名称</remarks>
        public DataBackgroundItemProxy( IDataManager dataManager ) : base(
            dataManager /*,todo: EnTableType.Content.ToString()*/)
        {
        }
        //...

        //todo:2写入自己代理的源数据模型属性
        public BackgroundItemTableModel BackgroundItemTableModel
        {
            get
            {
                var index = ConfigManager.Instance.CompanyIndex;
                return mModels[index];
            }
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
            SyncLoadAndInitModel(EnTableType.BackgroundItemTable.ToString(), true);
        }

        /// <summary>
        /// 代理移除
        /// </summary>
        public override void OnRemove()
        {
            base.OnRemove();
            mModels.Clear();
            mModels = null;
        }

        #endregion

        #region Internal Members

        //todo:写自身代理的处理的数据逻辑


        public override T GetData<T>( int id )
        {
            return BackgroundItemTableModel.GetBackgroundItemByItemID(id) as T;
        }

        /// <summary>
        /// 根据物体类型获取所有物体
        /// </summary>
        /// <param name="itemTypeName"></param>
        /// <returns></returns>
        public List<BackgroundItemDefine> GetDataByItemType( string itemTypeName )
        {

	        return string.IsNullOrEmpty(itemTypeName) ? null : BackgroundItemTableModel.BackgroundItem.FindAll(x=> { return x.ItemType == itemTypeName; } );
        }

        #endregion
    }
}