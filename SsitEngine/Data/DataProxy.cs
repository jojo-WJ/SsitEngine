/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：数据代理                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 13:42:55                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.Core;
using SsitEngine.PureMVC.Patterns;

namespace SsitEngine.Data
{
    /// <summary>
    ///     数据代理基类
    /// </summary>
    [Serializable]
    public abstract class DataProxy<T> : Proxy, IDataProxy where T : ModelBase
    {
        /// <summary>
        ///     数据管理器
        /// </summary>
        protected IDataManager m_dataManager;

        /// <summary>
        ///     数据模型
        /// </summary>
        protected Dictionary<string, T> mModels;

        /// <summary>
        ///     构造方法
        /// </summary>
        /// <param name="dataManager">数据管理器</param>
        /// <param name="name">数据代理的名称</param>
        public DataProxy( IDataManager dataManager, string name ) : base(name)
        {
            m_dataManager = dataManager;
            mModels = new Dictionary<string, T>();
        }

        public DataProxy( IDataManager dataManager ) : base(typeof(T).FullName)
        {
            m_dataManager = dataManager;
            mModels = new Dictionary<string, T>();
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetData<T>( int id ) where T : class
        {
            return null;
        }

        /// <inheritdoc />
        public override void OnRegister()
        {
            base.OnRegister();
        }


        /// <inheritdoc />
        public override void OnRemove()
        {
            base.OnRemove();
            m_dataManager = null;
        }

        /// <summary>
        ///     数据加载
        /// </summary>
        /// <typeparam name="T">加载数据模型</typeparam>
        /// <param name="tablePath">数据路径</param>
        /// <param name="isFlag">数据附加标识</param>
        /// <returns></returns>
        protected List<T> LoadAndInitModel<T>( string tablePath, bool isFlag = false ) where T : ModelBase
        {
            var modeLList = m_dataManager.Loader.Load<T>(tablePath, isFlag);
            return modeLList;
        }


        protected void SyncLoadAndInitModel( string tablePath, bool isFlag )
        {
            m_dataManager.Loader.Load<T>(tablePath, isFlag, data => { mModels = data; });
        }

        protected void SyncLoadAndInitModel<T1>( string tablePath, bool isFlag,
            SsitAction<Dictionary<string, T1>> func ) where T1 : ModelBase
        {
            m_dataManager.Loader.Load(tablePath, isFlag, func);
        }


        /// <summary>
        ///     更新数据
        /// </summary>
        /// <param name="dataList"></param>
        public abstract void UpdateData( List<DataBase> dataList );
    }
}