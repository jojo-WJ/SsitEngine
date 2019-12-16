using System.Collections.Generic;
using SsitEngine.Data;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
/**
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 12:04:51                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Data
{
    /// <summary>
    ///     数据管理器
    /// </summary>
    public class DataManager : ManagerBase<DataManager>, IDataManager
    {
        private Dictionary<int, IDataProxy> m_dataMaps = new Dictionary<int, IDataProxy>();

        /// <inheritdoc />
        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
        }

        #region Property

        /// <summary>
        ///     数据加载接口
        /// </summary>
        public IDataLoader Loader { get; private set; }

        public float LoadPrecent => Loader.LoadPrecent;

        #endregion

        #region Moudle

        /// <summary>
        ///     获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => (int) EnModuleType.ENMODULEDATA;


        /// <summary>
        ///     对象池管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public override void OnUpdate( float elapseSeconds )
        {
        }

        /// <summary>
        ///     关闭并清理对象池管理器。
        /// </summary>
        public override void Shutdown()
        {
            if (isShutdown)
                return;

            isShutdown = true;
            var mapIt = m_dataMaps.GetEnumerator();
            while (mapIt.MoveNext()) Facade.Instance.RemoveProxy(mapIt.Current.Value.ProxyName);
            mapIt.Dispose();
            m_dataMaps.Clear();
            m_dataMaps = null;
        }

        #endregion

        #region 数据加载

        /// <summary>
        ///     设置数据加载代理
        /// </summary>
        /// <param name="loader"></param>
        public void SetLoaderHelper( IDataLoader loader )
        {
            Loader = loader;
            var tempDic = loader.InitProxy(this);
            foreach (var tt in tempDic)
            {
                Facade.Instance.RegisterProxy(tt.Value as IProxy);
                m_dataMaps.Add(tt.Key, tt.Value);
            }
        }

        /// <summary>
        ///     添加数据代理
        /// </summary>
        /// <param name="index"></param>
        /// <param name="proxy"></param>
        public void AddDataProxy( int index, DataProxy<ModelBase> proxy )
        {
            if (!m_dataMaps.ContainsKey(index))
            {
                Facade.Instance.RegisterProxy(proxy);
                m_dataMaps.Add(index, proxy);
            }
        }

        /// <summary>
        ///     更新数据代理
        /// </summary>
        /// <typeparam name="T">数据代理类型</typeparam>
        /// <param name="index">索引</param>
        /// <param name="dataList">数据源</param>
        public void UpdateDataProxy<T>( int index, List<DataBase> dataList ) where T : DataProxy<ModelBase>
        {
            DataProxy<ModelBase> proxy = GetDataProxy<T>(index);
            proxy.UpdateData(dataList);
        }

        #endregion

        #region 数据查询

        /// <summary>
        ///     获取数据代理
        /// </summary>
        /// <param name="index">数据代理索引</param>
        /// <returns>数据代理</returns>
        public T GetDataProxy<T>( int index ) where T : class, IDataProxy
        {
            if (m_dataMaps.ContainsKey(index))
                return m_dataMaps[index] as T;
            return null;
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="index">数据索引</param>
        /// <param name="id">数据id</param>
        /// <returns>数据</returns>
        public T GetData<T>( int index, int id ) where T : class
        {
            if (m_dataMaps.ContainsKey(index)) return m_dataMaps[index].GetData<T>(id);
            return default;
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxyName">代理名称</param>
        /// <param name="id">数据id</param>
        /// <returns></returns>
        public T GetData<T>( string proxyName, int id ) where T : class
        {
            var proxy = Facade.Instance.RetrieveProxy(proxyName);
            if (proxy == null) throw new SsitEngineException(TextUtils.Format("查询的数据名称{0}代理不存在", proxyName));
            var dataProxy = proxy as DataProxy<ModelBase>;
            if (dataProxy == null) throw new SsitEngineException("查询的数据代理转换异常");

            return dataProxy.GetData<T>(id);
        }


        /// <summary>
        ///     获取所有数据代理
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, IDataProxy> GetAllDataProxy()
        {
            return m_dataMaps;
        }

        #endregion
    }
}