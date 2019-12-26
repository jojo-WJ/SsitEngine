using System;
using System.Collections;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Data;
using SsitEngine.Unity.Resource.Data;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SsitEngine.Unity.Resource
{
    /// <summary>
    ///     资源加载辅助器
    /// </summary>
    public class AssetLoaderHelper : IResourceLoaderHelper
    {
        // LRU缓存器
        //private readonly LRUCache<int, AssetInfo<Object>> m_lruCache = new LRUCache<int, AssetInfo<Object>>();

        public virtual void LoadAsset<T>( string resPath, UnityAction<T> callBack ) where T : Object
        {
            Load(resPath, callBack, () => { throw new Exception("资源加载异常: " + resPath + " 请确认资源是否存在"); });
        }

        public virtual void Shutdown()
        {
        }

        #region Load Form Resouces

        private void Load<T>( string path, UnityAction<T> complete, UnityAction FailedAction ) where T : Object
        {
            var tmpPath = PathUtils.GetPathWithoutFileExtension(path);
            var rr = Resources.Load<T>(tmpPath);
            if (rr)
            {
                complete?.Invoke(rr);
            }
            else
            {
                FailedAction?.Invoke();
            }
        }


        private IEnumerator SyncLoad<T>( string path, UnityAction<T> complete, UnityAction FailedAction )
            where T : Object
        {
            var tmpPath = PathUtils.GetPathWithoutFileExtension(path);
            var rr = Resources.LoadAsync(tmpPath, typeof(T));
            yield return rr;

            if (rr.isDone)
            {
                if (rr.asset == null)
                {
                    FailedAction?.Invoke();
                }
                else
                {
                    complete?.Invoke(rr.asset as T);
                }
            }
        }

        #endregion

        #region IResourceLoader Impl

        /// <inheritdoc />
        public virtual T LoadAsset<T>( int Id ) where T : Object
        {
            var resData = DataManager.Instance.GetData<IResourceData>((int) EnDataType.DATA_Res, Id);
            if (resData == null)
            {
                throw new Exception("Resources资源配置不存在：" + Id);
            }
            return LoadAsset<T>(resData.ResourcePath);
        }

        /// <inheritdoc />
        public virtual T LoadAsset<T>( string resPath ) where T : Object
        {
            return Resources.Load<T>(PathUtils.GetPathWithoutFileExtension(resPath));
        }

        /// <inheritdoc />
        public virtual void LoadAsset<T>( int Id, UnityAction<T> callBack ) where T : Object
        {
            var resData = DataManager.Instance.GetData<IResourceData>((int) EnDataType.DATA_Res, Id);
            if (resData == null)
            {
                throw new Exception("Resources资源配置不存在：" + Id);
            }
            LoadAsset(resData.ResourcePath, callBack);
        }

        /// <summary>
        /// 从Resource目录中加载资源
        /// </summary>
        /// <param name="id">资源的ID标识</param>
        /// <param name="callBack">加载完成回调</param>
        public virtual void SyncLoadAsset<T>( int id, UnityAction<T> callBack ) where T : Object
        {
            var resData = DataManager.Instance.GetData<IResourceData>((int) EnDataType.DATA_Res, id);
            if (resData == null)
            {
                throw new Exception("Resources资源配置不存在：" + id);
            }
            SyncLoadAsset(resData.ResourcePath, callBack);
        }

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="resPath">资源的相对路径</param>
        /// <returns>未实例化的资源</returns>
        public virtual void SyncLoadAsset<T>( string resPath, UnityAction<T> callBack ) where T : Object
        {
            Engine.Instance.Platform.StartPlatCoroutine(SyncLoad(resPath, callBack,
                () => { throw new Exception("资源加载异常: " + resPath + " 请确认资源是否存在"); }));
        }


        /// <inheritdoc />
        public virtual void UnloadAsset<T>( Object obj )
        {
        }

        #endregion

        #region Spirte Atlas Loader

        public virtual void LoadSpriteAsset( int spriteId, UnityAction<Sprite> loadSpriteFunc )
        {
            SsitDebug.Error("上层自行实现");
        }

        public virtual void LoadSpriteAtlasAsset( int atlasId, string spriteName, UnityAction<Sprite> loadSpriteFunc )
        {
            SsitDebug.Error("上层自行实现");
        }

        public virtual void SyncLoadAddressableSpriteAtlas( int atlasId )
        {
        }

        public virtual void UnLoadSpriteAtlasAsset( int atlasId )
        {
            SsitDebug.Error("上层自行实现");
        }

        #endregion
    }
}