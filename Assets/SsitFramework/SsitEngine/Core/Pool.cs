using System;
using System.Collections.Generic;
using SsitEngine.DebugLog;

namespace SsitEngine.Core
{
    /// <summary>
    ///     对象池simple
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PoolDataBase<T> where T : new()
    {
        /// <summary>
        ///     创建一个对象代理
        /// </summary>
        /// <param name="tmpData">代理数据</param>
        public PoolDataBase( T tmpData )
        {
            Data = tmpData;
            IsUsing = false;
        }

        public PoolDataBase()
        {
            Data = default;

            if (Data == null)
            {
                Data = new T();
            }
            IsUsing = false;
        }

        public T Data { get; set; }

        public bool IsUsing { get; set; }
    }

    /// <summary>
    ///     泛型对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : new()
    {
        private static Pool<T> instance;
        protected Func<int, T, bool> condition;

        protected Func<T> loadFunc0;
        protected Func<int, T> loadFunc1;

        private List<PoolDataBase<T>> objectManager;
        protected Action<T> OnDestoryAction;

        protected int sameExistCount = 2;

        /// <summary>
        ///     创建对象池
        /// </summary>
        public Pool()
        {
            objectManager = new List<PoolDataBase<T>>();
            loadFunc0 = null;
            loadFunc1 = null;
            sameExistCount = 2;
        }


        /// <summary>
        ///     池对象单例
        /// </summary>
        public static Pool<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Pool<T>();
                }

                return instance;
            }
        }

        /// <summary>
        ///     初始化对象池
        /// </summary>
        /// <param name="sameExistCount">缓存容量</param>
        /// <param name="loadFunc"></param>
        public void Init( int sameExistCount, Func<T> loadFunc = null )
        {
            this.sameExistCount = sameExistCount;
            loadFunc0 = loadFunc;
        }

        /// <summary>
        ///     初始化对象池
        /// </summary>
        /// <param name="sameExistCount"></param>
        /// <param name="loadFunc"></param>
        public void Init( int sameExistCount, Func<T> loadFunc, Action<T> destoryAction )
        {
            this.sameExistCount = sameExistCount;
            loadFunc0 = loadFunc;
            OnDestoryAction = destoryAction;
        }


        /// <summary>
        ///     初始化对象池
        /// </summary>
        /// <param name="sameExistCount"></param>
        /// <param name="loadFunc"></param>
        /// <param name="condition"></param>
        public void Init( int sameExistCount, Func<int, T> loadFunc = null, Func<int, T, bool> condition = null )
        {
            this.sameExistCount = sameExistCount;
            loadFunc1 = loadFunc;
            this.condition = condition;
        }

        /// <summary>
        ///     创建对象池
        /// </summary>
        /// <param name="sameExistCount"></param>
        /// <param name="loadFunc"></param>
        public static Pool<T> CreatePool( int sameExistCount, Func<T> loadFunc = null )
        {
            var pool = Instance;
            pool.Init(sameExistCount, loadFunc);
            return pool;
        }

        /// <summary>
        ///     创建对象池
        /// </summary>
        /// <param name="sameExistCount"></param>
        /// <param name="loadFunc"></param>
        public static Pool<T> CreatePool( int sameExistCount, Func<T> loadFunc, Action<T> destoryAction )
        {
            var pool = Instance;
            pool.Init(sameExistCount, loadFunc, destoryAction);
            return pool;
        }

        /// <summary>
        ///     创建对象池
        /// </summary>
        /// <param name="sameExistCount"></param>
        /// <param name="loadFunc"></param>
        /// <param name="condition"></param>
        public static Pool<T> CreatePool( int sameExistCount, Func<int, T> loadFunc = null,
            Func<int, T, bool> condition = null )
        {
            var pool = Instance;
            pool.Init(sameExistCount, loadFunc, condition);
            return pool;
        }


        /// <summary>
        ///     销毁池
        /// </summary>
        public void Destroy()
        {
            if (objectManager != null)
            {
                objectManager.Clear();
            }

            objectManager = null;
            loadFunc0 = null;
            loadFunc1 = null;
            instance = null;
        }

        /// <summary>
        ///     析构
        /// </summary>
        ~Pool()
        {
            Destroy();
        }

        /// <summary>
        ///     获取随机空闲的对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <returns>指定对象</returns>
        public T GetFreeObject()
        {
            for (var i = 0; i < objectManager.Count; i++)
            {
                if (!objectManager[i].IsUsing)
                {
                    objectManager[i].IsUsing = true;
                    return objectManager[i].Data;
                }
            }
            PoolDataBase<T> tmpData = null;
            if (loadFunc0 != null)
            {
                tmpData = new PoolDataBase<T>(loadFunc0());
            }
            else
            {
                tmpData = new PoolDataBase<T>();
            }

            tmpData.IsUsing = true;
            objectManager.Add(tmpData);


            return tmpData.Data;
        }

        /// <summary>
        ///     获取随机空闲的对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <returns>指定对象</returns>
        public T GetFreeObject( int id )
        {
            for (var i = 0; i < objectManager.Count; i++)
            {
                if (!objectManager[i].IsUsing)
                {
                    var isRet = !(condition != null && !condition(id, objectManager[i].Data));
                    if (isRet)
                    {
                        objectManager[i].IsUsing = true;
                        return objectManager[i].Data;
                    }
                }
            }
            var data = loadFunc1(id);
            if (data == null)
            {
                SsitDebug.Error("回调为空");
            }
            var tmpData = new PoolDataBase<T>(data);

            tmpData.IsUsing = true;
            objectManager.Add(tmpData);


            return tmpData.Data;
        }

        /// <summary>
        ///     释放对象
        /// </summary>
        /// <param name="data"></param>
        public virtual void ReleaseObject( T data )
        {
            for (var i = 0; i < objectManager.Count; i++)
            {
                if (data.Equals(objectManager[i].Data))
                {
                    objectManager[i].IsUsing = false;
                    break;
                }
            }
            ClearPool();
        }

        /// <summary>
        ///     清理池
        /// </summary>
        public virtual void ClearPool()
        {
            ushort tmpCount = 0;
            var clearList = new List<PoolDataBase<T>>();
            for (var i = 0; i < objectManager.Count; i++)
            {
                if (!objectManager[i].IsUsing)
                {
                    tmpCount++;
                    if (tmpCount > sameExistCount)
                    {
                        clearList.Add(objectManager[i]);
                    }
                }
            }

            for (var i = 0; i < clearList.Count; i++)
            {
                objectManager.Remove(clearList[i]);
                if (OnDestoryAction != null)
                {
                    OnDestoryAction(objectManager[i].Data);
                }
            }
        }
    }
}